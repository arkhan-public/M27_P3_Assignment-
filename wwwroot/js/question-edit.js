// Edit Question Page JavaScript
(function () {
    'use strict';

    // Page Data (will be set from Razor Page)
    let pageData = window.questionEditData || {};

    function initialize() {
        const token = localStorage.getItem('jwtToken');
        const currentUserId = Number(localStorage.getItem('userId'));
        const ownerId = pageData.ownerId || 0;
        const questionId = pageData.questionId || 0;

        // Redirect to login if not authenticated
        if (!token) {
            window.location.href = '/Login';
            return;
        }

        // Only owner may edit (UI check). Server enforces ownership on PUT.
        if (currentUserId !== ownerId) {
            window.location.href = '/Questions/Details?id=' + questionId;
            return;
        }

        const form = document.getElementById('editQuestionForm');
        if (form) {
            form.addEventListener('submit', handleSubmit);
        }
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const token = localStorage.getItem('jwtToken');
        const questionId = pageData.questionId || 0;
        const title = document.getElementById('title').value;
        const body = document.getElementById('body').value;
        const tags = document.getElementById('tags').value;

        if (title.length < 10) {
            document.getElementById('errorMessage').textContent = 'Title must be at least 10 characters';
            document.getElementById('errorMessage').classList.remove('d-none');
            return;
        }

        if (body.length < 20) {
            document.getElementById('errorMessage').textContent = 'Body must be at least 20 characters';
            document.getElementById('errorMessage').classList.remove('d-none');
            return;
        }

        try {
            const response = await fetch('/api/questions/' + questionId, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
                body: JSON.stringify({ title, body, tags })
            });

            if (response.ok) {
                document.getElementById('successMessage').textContent = 'Question updated successfully!';
                document.getElementById('successMessage').classList.remove('d-none');
                setTimeout(() => {
                    window.location.href = '/Questions/Details?id=' + questionId;
                }, 1000);
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                const data = await response.json();
                document.getElementById('errorMessage').textContent = data.message || 'Failed to update question';
                document.getElementById('errorMessage').classList.remove('d-none');
            }
        } catch (err) {
            document.getElementById('errorMessage').textContent = 'An error occurred. Please try again.';
            document.getElementById('errorMessage').classList.remove('d-none');
        }
    }

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', initialize);
})();