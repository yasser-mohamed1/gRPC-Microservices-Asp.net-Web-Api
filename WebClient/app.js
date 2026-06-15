let cart = {}; // itemId -> { name, price, quantity }
let totalPrice = 0;

function addItem(id, name, price) {
    if (cart[id]) {
        cart[id].quantity += 1;
    } else {
        cart[id] = { name, price, quantity: 1 };
    }
    updateCartUI();
    log("Added " + name + " to cart.");
}

function removeItem(id) {
    if (cart[id]) {
        cart[id].quantity -= 1;
        if (cart[id].quantity <= 0) {
            delete cart[id];
        }
    }
    updateCartUI();
    log("Removed item from cart.");
}

function updateCartUI() {
    const list = document.getElementById("cart-list");
    const emptyMsg = document.getElementById("empty-cart-msg");
    const totalSpan = document.getElementById("total-price");
    
    const entries = Object.entries(cart);
    totalPrice = 0;
    
    if (entries.length === 0) {
        list.style.display = "none";
        emptyMsg.style.display = "block";
    } else {
        list.style.display = "block";
        emptyMsg.style.display = "none";
        
        list.innerHTML = entries.map(([id, item]) => {
            totalPrice += item.price * item.quantity;
            return `<li>${item.name} x ${item.quantity} ($${item.price * item.quantity}) <button onclick="removeItem('${id}')" style="margin-left:10px; font-size:11px;">Remove</button></li>`;
        }).join("");
    }
    totalSpan.textContent = totalPrice;
}

async function sendOrder() {
    const grpcEndpoint = document.getElementById("grpc-endpoint").value.trim();
    const userId = parseInt(document.getElementById("user-id").value) || 1;
    const items = Object.entries(cart).map(([id, item]) => ({
        itemId: id,
        quantity: item.quantity
    }));

    if (items.length === 0) {
        log("Error: Cart is empty. Please add items first.");
        return;
    }

    log("Sending submitOrder request via gRPC node client...");
    const btn = document.getElementById("btn-submit");
    btn.disabled = true;

    try {
        const hostUrl = window.location.port === "3000" ? "" : "http://localhost:3000";
        const response = await fetch(hostUrl + "/submit-order", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                grpcEndpoint,
                userId,
                totalPrice,
                items
            })
        });

        const result = await response.json();
        if (result.success) {
            log("SUCCESS: " + result.message);
        } else {
            log("FAILED: " + result.message);
        }
    } catch (error) {
        log("CONNECTION ERROR: " + error.message);
    } finally {
        btn.disabled = false;
    }
}

function log(msg) {
    const logDiv = document.getElementById("log");
    const time = new Date().toLocaleTimeString();
    logDiv.innerHTML += `\n[${time}] ${msg}`;
    logDiv.scrollTop = logDiv.scrollHeight;
}
