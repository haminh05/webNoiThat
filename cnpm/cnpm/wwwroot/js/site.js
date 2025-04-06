// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
/*
    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".product-card").forEach(card => {
            card.addEventListener("click", function (event) {
                // Nếu click vào "Mua ngay" hoặc "Thêm vào giỏ hàng" thì không chuyển trang
                if (event.target.closest(".btn-buy-now, .btn-add-to-cart")) {
                    event.stopPropagation();
                    return;
                }
                // Chuyển hướng đến trang chi tiết sản phẩm
                window.location.href = this.dataset.url;
            });
        });
    });
    */



//xu ly gio hang
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".add-to-cart").forEach(button => {
        button.addEventListener("click", function () {
            let productId = this.getAttribute("data-id");
            let productName = this.getAttribute("data-name");
            let productImage = this.getAttribute("data-image");
            let productPrice = this.getAttribute("data-price");

            fetch("/Cart/AddToCart", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    ProductId: productId,
                    ProductName: productName,  // Đổi từ Name → ProductName
                    ImageUrl: productImage,    // Đổi từ Image → ImageUrl
                    Price: productPrice,
                    Quantity: 1                // Thêm số lượng mặc định là 1
                })


            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert("✅ Đã thêm vào giỏ hàng!");
                        document.getElementById("cart-count").innerText = data.cartItemCount;
                    } else {
                        alert("❌ Lỗi khi thêm vào giỏ hàng!");
                    }
                })
                .catch(error => console.error("Lỗi:", error));
        });
    });
});


//đếm sp trong giỏ
document.addEventListener("DOMContentLoaded", function () {
    fetch("/Cart/GetCartItemCount")
        .then(response => response.json())
        .then(data => document.getElementById("cart-count").innerText = data.cartItemCount);
});



document.addEventListener("DOMContentLoaded", function () {
    // Toggle danh mục sản phẩm
    document.getElementById("toggle-category").addEventListener("click", function () {
        let categoryList = document.getElementById("category-list");
        categoryList.style.display = categoryList.style.display === "none" ? "block" : "none";
    });

    // Toggle chat box
    document.getElementById("toggle-chat").addEventListener("click", function () {
        let chatBox = document.getElementById("customer-chat");
        chatBox.style.display = chatBox.style.display === "none" ? "block" : "none";
    });

    // Xử lý gửi tin nhắn
    document.getElementById("send-message").addEventListener("click", function () {
        let input = document.getElementById("message-input");
        let message = input.value.trim();
        if (message !== "") {
            let chatBox = document.getElementById("chat-box");
            let newMessage = document.createElement("div");
            newMessage.style.padding = "5px";
            newMessage.style.background = "#dff9fb";
            newMessage.style.marginBottom = "5px";
            newMessage.style.borderRadius = "5px";
            newMessage.textContent = "Bạn: " + message;
            chatBox.appendChild(newMessage);
            chatBox.scrollTop = chatBox.scrollHeight; // Cuộn xuống tin nhắn mới nhất
            input.value = "";
        }
    });
});







////   CHAT   //////////////
$(document).ready(function () {
    var customerId = null;

    // Lấy userId
    $.get("/api/chat/getUserId", function (data) {
        if (data.userId) {
            customerId = data.userId;
            console.log("UserId từ session:", customerId);
            loadMessages(); // Chỉ load tin nhắn khi có customerId hợp lệ
        } else {
            console.log("Không tìm thấy UserId.");
            $("#chat-box").html("<p class='text-muted'>Vui lòng đăng nhập để sử dụng chat.</p>");
        }
    }).fail(function () {
        console.log("Lỗi khi gọi API /api/getUserId");
    });

    // Kết nối SignalR
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    connection.start()
        .then(function () {
            console.log("✅ SignalR connected!");
        })
        .catch(function (err) {
            console.error("❌ SignalR connection error:", err.toString());
        });

    connection.onclose(function () {
        console.warn("⚠️ SignalR disconnected. Reconnecting...");
        setTimeout(function () {
            connection.start();
        }, 5000);
    });

    // Nhận tin nhắn từ nhân viên
    connection.on("ReceiveMessage", function (senderId, message, senderName) {
        $("#chat-box").append(
            `<div><strong>${senderName}:</strong> ${message} <i>(Vừa nhận)</i></div>`
        );
        scrollChatToBottom();
    });

    var userName = $("#currentUser").data("username") || "Khách hàng";

    // Gửi tin nhắn
    $("#send-message").click(function () {
        var message = $("#message-input").val();

        if (!customerId || customerId === "null" || parseInt(customerId) <= 0) {
            console.error("❌ Lỗi: SenderId (customerId) không hợp lệ!", customerId);
            return;
        }

        var receiverId = 13;
        var receiverName = "Nhân viên hỗ trợ";

        var messageData = {
            SenderId: customerId,
            SenderName: userName,
            ReceiverId: receiverId,
            ReceiverName: receiverName,
            Message: message
        };

        console.log("✅ Dữ liệu gửi đi:", messageData);

        $.ajax({
            url: "https://localhost:7061/api/chat/send",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(messageData),
            success: function () {
                console.log("✔ Tin nhắn đã lưu.");
                $("#message-input").val("");

                $("#chat-box").append(
                    `<div><strong>${userName}:</strong> ${message}</div>`
                );

                scrollChatToBottom();
                loadMessages();
            },
            error: function (xhr) {
                console.error("❌ Lỗi khi gửi tin nhắn:", xhr.responseText);
            }
        });
    });

    // Hàm cuộn xuống cuối khung chat
    function scrollChatToBottom() {
        var chatBox = $("#chat-box");
        chatBox.scrollTop(chatBox[0].scrollHeight);
    }

    // Hàm tự động load tin nhắn
    function loadMessages() {
        if (!customerId || customerId === "null") return;
        $.get(`/api/chat/employee/${customerId}`, function (data) {
            $("#chat-box").html("");
            data.forEach(function (msg) {
                $("#chat-box").append(
                    `<div><strong>${msg.senderName}:</strong> ${msg.message} <i>(${new Date(msg.timestamp).toLocaleTimeString()})</i></div>`
                );
            });
        }).fail(function () {
            console.error("❌ Lỗi khi tải tin nhắn từ server!");
        });
    }

    loadMessages();
});
