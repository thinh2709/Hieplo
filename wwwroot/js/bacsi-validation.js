$(document).ready(function() {
    // Regex cho email với các ký tự Unicode và tên miền hợp lệ
    const EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    
    function isValidEmail(email) {
        if (!email) return false;
        return EMAIL_REGEX.test(email.trim());
    }

    function showError(fieldId, message) {
        $(`#${fieldId}`).addClass('is-invalid');
        const errorDiv = $(`#${fieldId}`).siblings('.text-danger');
        errorDiv.text(message);
    }

    function clearError(fieldId) {
        $(`#${fieldId}`).removeClass('is-invalid');
        const errorDiv = $(`#${fieldId}`).siblings('.text-danger');
        errorDiv.text('');
    }

    $("#formBacSi").submit(function(event) {
        let isValid = true;
        const requiredFields = {
            "HoTen": "Họ tên",
            "GioiTinh": "Giới tính",
            "ChuyenKhoa": "Chuyên khoa",
            "MaKhoa": "Khoa",
            "SoDienThoai": "Số điện thoại",
            "Email": "Email"
        };

        // Reset all errors
        Object.keys(requiredFields).forEach(field => {
            clearError(field);
        });

        // Kiểm tra các trường bắt buộc
        Object.keys(requiredFields).forEach(field => {
            const value = $(`#${field}`).val();
            if (!value || value.trim() === '') {
                isValid = false;
                showError(field, `${requiredFields[field]} không được để trống`);
            }
        });

        // Kiểm tra số điện thoại
        const phone = $("#SoDienThoai").val();
        if (phone) {
            if (!phone.match(/^[0-9]{10}$/)) {
                isValid = false;
                showError("SoDienThoai", "Số điện thoại phải có đúng 10 chữ số");
            }
        }

        // Kiểm tra email
        const email = $("#Email").val();
        if (email) {
            if (!isValidEmail(email)) {
                isValid = false;
                showError("Email", "Email không đúng định dạng (ví dụ: example@domain.com)");
            }
        }

        if (!isValid) {
            event.preventDefault();
            $('html, body').animate({
                scrollTop: $('.is-invalid').first().offset().top - 100
            }, 500);
        }
    });

    // Xóa thông báo lỗi khi người dùng thay đổi giá trị
    $("#formBacSi input, #formBacSi select").on('input change', function() {
        clearError($(this).attr('id'));
    });
}); 