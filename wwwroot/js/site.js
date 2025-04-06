// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Xử lý đăng xuất
document.addEventListener('DOMContentLoaded', function() {
    // Tìm tất cả các nút/liên kết đăng xuất
    const logoutLinks = document.querySelectorAll('a[href*="Logout"], button[data-action="logout"]');
    
    // Tạo một form đăng xuất ẩn nếu chưa tồn tại
    let logoutForm = document.getElementById('logoutForm');
    if (!logoutForm) {
        logoutForm = document.createElement('form');
        logoutForm.id = 'logoutForm';
        logoutForm.method = 'post';
        logoutForm.action = '/Account/Logout';
        logoutForm.style.display = 'none';
        
        // Thêm token CSRF
        const antiforgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
        if (antiforgeryToken) {
            const tokenInput = antiforgeryToken.cloneNode(true);
            logoutForm.appendChild(tokenInput);
        }
        
        document.body.appendChild(logoutForm);
    }
    
    // Thêm sự kiện click cho các nút đăng xuất
    logoutLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            // Ngăn chặn hành vi mặc định
            e.preventDefault();
            
            // Submit form đăng xuất
            logoutForm.submit();
        });
    });
});
