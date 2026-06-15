const express = require('express');
const path = require('path');
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');

// Ignore self-signed certificate errors for local gRPC HTTPS endpoints
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const app = express();
const PORT = 3000;

app.use(express.json());
app.use((req, res, next) => {
    res.header("Access-Control-Allow-Origin", "*");
    res.header("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
    res.header("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
    if (req.method === "OPTIONS") {
        return res.sendStatus(200);
    }
    next();
});
app.use(express.static(__dirname));

// POST endpoint that acts as the gRPC client
app.post('/submit-order', (req, res) => {
    try {
        const { grpcEndpoint, userId, totalPrice, items } = req.body;

        const PROTO_PATH = path.join(__dirname, '../OrderingService/Protos/order.proto');
        
        // Load the order proto
        const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
            keepCase: true,
            longs: String,
            enums: String,
            defaults: true,
            oneofs: true
        });

        const protoDescriptor = grpc.loadPackageDefinition(packageDefinition);
        
        // Navigate through package namespace
        const servicePackage = protoDescriptor.OrderingService && protoDescriptor.OrderingService.Protos;
        const serviceClientClass = servicePackage ? servicePackage.OrderingGrpcService : protoDescriptor.OrderingGrpcService;

        if (!serviceClientClass) {
            return res.status(500).json({ 
                success: false, 
                message: "Failed to locate OrderingGrpcService in loaded proto descriptor." 
            });
        }

        // Determine credentials (use secure channel for https, insecure for http)
        const isHttps = grpcEndpoint.startsWith('https://') || grpcEndpoint.includes('7153');
        const cleanAddress = grpcEndpoint.replace(/^https?:\/\//, '');

        const credentials = isHttps 
            ? grpc.credentials.createSsl() 
            : grpc.credentials.createInsecure();

        console.log(`Connecting via gRPC to: ${cleanAddress} (${isHttps ? 'Secure' : 'Insecure'})`);

        const client = new serviceClientClass(
            cleanAddress, 
            credentials
        );

        const grpcPayload = {
            userId: parseInt(userId) || 0,
            totalPrice: parseInt(totalPrice) || 0,
            items: (items || []).map(item => ({
                itemId: item.itemId,
                quantity: parseInt(item.quantity) || 0
            }))
        };

        // Call the C# gRPC service
        client.SubmitOrder(grpcPayload, (error, response) => {
            if (error) {
                console.error('gRPC Error:', error);
                return res.status(500).json({ 
                    success: false, 
                    message: `gRPC Service Error: ${error.message}` 
                });
            }
            res.json({
                success: response.success,
                message: response.message
            });
        });
    } catch (err) {
        console.error('Server Handler Error:', err);
        res.status(500).json({
            success: false,
            message: `Express Server Error: ${err.message}`
        });
    }
});

app.listen(PORT, () => {
    console.log(`Web Client server running at http://localhost:${PORT}`);
});
