document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");

    const fields = {
        fullName: form.querySelector('input[placeholder="Full name"]'),
        email: form.querySelector('input[placeholder="Email"]'),
        password: form.querySelectorAll('input[placeholder="Password"]')[0],
        confirmPassword: form.querySelector('input[placeholder="Confirm password"]')
    };

    // Function to show error message below input field
    function showError(input, message) {
        const span = input.parentElement.querySelector(".error-message");
        span.textContent = message || "";
    }

    form.addEventListener("submit", function (e) {
        let valid = true;

        // Clear all previous error messages
        Object.values(fields).forEach(input => showError(input, ""));

        // Full name validation
        const namePattern = /^[a-zA-Z\s]{2,}$/;
        if (!namePattern.test(fields.fullName.value.trim())) {
            valid = false;
            showError(fields.fullName, "Please enter a valid full name (letters only).");
        }

        // Email validation
        const emailPattern = /^[^ ]+@[^ ]+\.[a-z]{2,3}$/;
        if (!emailPattern.test(fields.email.value)) {
            valid = false;
            showError(fields.email, "Enter a valid email address.");
        }

        // Password validation
        const passwordValue = fields.password.value;
        const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/;
        if (!passwordPattern.test(passwordValue)) {
            valid = false;
            showError(fields.password, "Password must be at least 6 characters long and contain an uppercase letter, a lowercase letter, and a number.");
        }

        // Confirm password validation
        if (fields.password.value !== fields.confirmPassword.value) {
            valid = false;
            showError(fields.confirmPassword, "Passwords do not match.");
        }

        if (!valid) {
            e.preventDefault(); // Prevent form submission if validation fails
        }
    });
});
