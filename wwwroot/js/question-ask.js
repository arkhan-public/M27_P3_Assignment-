// Ask Question Page JavaScript
(function () {
    'use strict';

    function initialize() {
        // Redirect if not logged in
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        const form = document.getElementById('askQuestionForm');
        if (form) {
            form.addEventListener('submit', handleSubmit);
        }
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const title = document.getElementById('title').value;
        const body = document.getElementById('body').value;
        const tags = document.getElementById('tags').value;

        try {
            const response = await fetch('/api/questions', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getToken()
                },
                body: JSON.stringify({ title, body, tags })
            });

            if (response.ok) {
                const data = await response.json();
                document.getElementById('successMessage').textContent = 'Question posted successfully!';
                document.getElementById('successMessage').classList.remove('d-none');
                setTimeout(() => {
                    window.location.href = '/Questions/Details?id=' + data.questionId;
                }, 1000);
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                const data = await response.json();
                document.getElementById('errorMessage').textContent = data.message || 'Failed to post question';
                document.getElementById('errorMessage').classList.remove('d-none');
            }
        } catch (error) {
            document.getElementById('errorMessage').textContent = 'An error occurred. Please try again.';
            document.getElementById('errorMessage').classList.remove('d-none');
        }
    }

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', initialize);
})();