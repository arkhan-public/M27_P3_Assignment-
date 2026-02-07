// Edit Answer Page JavaScript
(function () {
    'use strict';

    // Page Data (will be set from Razor Page)
    let pageData = window.answerEditData || {};

    function initialize() {
        const token = getToken();
        
        console.log('Initializing answer edit page...');
        console.log('Page data:', pageData);
        console.log('Has token:', !!token);

        if (!token) {
            console.error('No authentication token found - redirecting to login');
            window.location.href = '/Login';
            return;
        }

        const form = document.getElementById('editAnswerForm');
        if (form) {
            console.log('Edit form found, attaching submit handler');
            form.addEventListener('submit', handleSubmit);
        } else {
            console.error('Edit form not found!');
        }
    }

    async function handleSubmit(e) {
        e.preventDefault();

        const token = getToken();
        const answerId = pageData.answerId || 0;
        const questionId = pageData.questionId || 0;
        const bodyElement = document.getElementById('body');
        const errorElement = document.getElementById('errorMessage');
        const body = bodyElement.value.trim();

        // Clear previous errors
        errorElement.classList.add('d-none');
        errorElement.textContent = '';

        console.log('Submitting answer edit:', { answerId, questionId, bodyLength: body.length });

        // Validation
        if (!body || body.length < 20) {
            errorElement.textContent = 'Answer must be at least 20 characters';
            errorElement.classList.remove('d-none');
            console.error('Validation failed: body too short');
            return;
        }

        if (!answerId || answerId === 0) {
            errorElement.textContent = 'Invalid answer ID';
            errorElement.classList.remove('d-none');
            console.error('Invalid answer ID:', answerId);
            return;
        }

        try {
            const response = await fetch('/api/answers/' + answerId, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
                body: JSON.stringify({ body: body })
            });

            console.log('Response status:', response.status);

            if (response.ok) {
                console.log('Answer updated successfully, redirecting...');
                window.location.href = '/Questions/Details?id=' + questionId;
            } else if (response.status === 401) {
                console.error('Unauthorized - redirecting to login');
                window.location.href = '/Login';
            } else {
                const data = await response.json().catch(() => ({ message: 'Failed to update answer' }));
                console.error('Error response:', data);
                errorElement.textContent = data.message || 'Failed to update answer';
                errorElement.classList.remove('d-none');
            }
        } catch (err) {
            console.error('Exception updating answer:', err);
            errorElement.textContent = 'An error occurred. Please try again.';
            errorElement.classList.remove('d-none');
        }
    }

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', initialize);
})();