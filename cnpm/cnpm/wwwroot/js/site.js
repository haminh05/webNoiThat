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

