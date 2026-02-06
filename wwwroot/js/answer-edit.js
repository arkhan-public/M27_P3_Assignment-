// Edit Answer Page JavaScript
(function () {
    'use strict';

    // Page Data (will be set from Razor Page)
    let pageData = window.answerEditData || {};

    function initialize() {
        const token = localStorage.getItem('jwtToken');
        if (!token) {
            window.location.href = '/Login';
            return;
        }

        const form = document.getElementById('editAnswerForm');
        if (form) {
            form.addEventListener('submit', handleSubmit);
        }
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const token = localStorage.getItem('jwtToken');
        const answerId = pageData.answerId || 0;
        const questionId = pageData.questionId || 0;
        const body = document.getElementById('body').value;

        if (body.length < 20) {
            document.getElementById('errorMessage').textContent = 'Answer must be at least 20 characters';
            document.getElementById('errorMessage').classList.remove('d-none');
            return;
        }

        try {
            const response = await fetch('/api/answers/' + answerId, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
                body: JSON.stringify({ body })
            });

            if (response.ok) {
                window.location.href = '/Questions/Details?id=' + questionId;
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                const data = await response.json();
                document.getElementById('errorMessage').textContent = data.message || 'Failed to update answer';
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