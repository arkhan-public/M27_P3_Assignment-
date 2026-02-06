// Question Details Page JavaScript
(function () {
    'use strict';

    // Page Data (will be set from Razor Page)
    let pageData = window.questionDetailsData || {};

    function initializeOwnershipControls() {
        const token = getToken();
        const currentUserId = Number(localStorage.getItem('userId'));
        const questionOwnerId = pageData.questionOwnerId || 0;

        // Show question-level controls if logged in and owner
        if (token && currentUserId === questionOwnerId) {
            const editBtn = document.getElementById('editQuestionBtn');
            const deleteBtn = document.getElementById('deleteQuestionBtn');
            if (editBtn) editBtn.style.display = 'inline-block';
            if (deleteBtn) deleteBtn.style.display = 'inline-block';

            // Show accept buttons for answers
            document.querySelectorAll('[id^="accept-"]').forEach(btn => btn.style.display = 'inline-block');
        }

        // Show per-answer Edit/Delete for answer owners using data-owner attributes
        document.querySelectorAll('[id^="editAnswer-"]').forEach(btn => {
            const ownerId = Number(btn.getAttribute('data-owner'));
            const parts = btn.id.split('-');
            const aid = parts[1];
            const delBtn = document.getElementById('deleteAnswer-' + aid);

            if (token && ownerId === currentUserId) {
                btn.style.display = 'inline-block';
                if (delBtn) delBtn.style.display = 'inline-block';
            } else {
                btn.style.display = 'none';
                if (delBtn) delBtn.style.display = 'none';
            }
        });
    }

    function attachAnswerDeleteHandlers() {
        document.querySelectorAll('[id^="deleteAnswer-"]').forEach(btn => {
            btn.addEventListener('click', async function () {
                const answerId = this.id.split('-')[1];
                if (!confirm('Are you sure you want to delete this answer?')) return;
                
                try {
                    const response = await fetch('/api/answers/' + answerId, {
                        method: 'DELETE',
                        headers: {
                            'Authorization': 'Bearer ' + getToken()
                        }
                    });

                    if (response.ok) {
                        location.reload();
                    } else if (response.status === 401) {
                        window.location.href = '/Login';
                    } else {
                        const data = await response.json();
                        alert(data.message || 'Failed to delete answer');
                    }
                } catch (err) {
                    console.error(err);
                    alert('An error occurred. Please try again.');
                }
            });
        });
    }

    function attachQuestionDeleteHandler() {
        const deleteBtnEl = document.getElementById('deleteQuestionBtn');
        const questionId = pageData.questionId || 0;

        if (deleteBtnEl) {
            deleteBtnEl.addEventListener('click', async function () {
                if (!confirm('Are you sure you want to delete this question? This cannot be undone.')) return;
                
                try {
                    const response = await fetch('/api/questions/' + questionId, {
                        method: 'DELETE',
                        headers: {
                            'Authorization': 'Bearer ' + getToken()
                        }
                    });

                    if (response.ok) {
                        window.location.href = '/Questions';
                    } else if (response.status === 401) {
                        window.location.href = '/Login';
                    } else {
                        const data = await response.json();
                        alert(data.message || 'Failed to delete question');
                    }
                } catch (err) {
                    console.error(err);
                    alert('An error occurred. Please try again.');
                }
            });
        }
    }

    window.acceptAnswer = async function (answerId) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        if (!confirm('Accept this answer as the correct one?')) return;

        try {
            const response = await fetch('/api/answers/' + answerId + '/accept', {
                method: 'POST',
                headers: {
                    'Authorization': 'Bearer ' + getToken()
                }
            });

            if (response.ok) {
                location.reload();
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                const data = await response.json();
                alert(data.message || 'Failed to accept answer');
            }
        } catch (err) {
            console.error(err);
            alert('An error occurred. Please try again.');
        }
    };

    function initializeAnswerForm() {
        if (getToken()) {
            const answerForm = document.getElementById('answerForm');
            const commentBtn = document.getElementById('show-question-comment-btn');
            if (answerForm) answerForm.style.display = 'block';
            if (commentBtn) commentBtn.style.display = 'inline-block';
        }
    }

    window.showCommentForm = function (type, id) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        if (type === 'question') {
            document.getElementById('question-comment-form').style.display = 'block';
            document.getElementById('show-question-comment-btn').style.display = 'none';
        } else {
            document.getElementById('answer-' + id + '-comment-form').style.display = 'block';
        }
    };

    window.vote = async function (questionId, answerId, voteType) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        try {
            const response = await fetch('/api/vote', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getToken()
                },
                body: JSON.stringify({
                    questionId,
                    answerId,
                    type: voteType
                })
            });

            if (response.ok) {
                location.reload();
            } else if (response.status === 401) {
                window.location.href = '/Login';
            }
        } catch (error) {
            console.error('Error voting:', error);
        }
    };

    window.submitAnswer = async function () {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        const bodyElement = document.getElementById('answerBody');
        const errorElement = document.getElementById('answerError');
        const body = bodyElement.value.trim();
        const questionId = pageData.questionId || 0;

        // Clear previous errors
        errorElement.classList.add('d-none');
        errorElement.textContent = '';

        // Validation
        if (!body || body.length < 20) {
            errorElement.textContent = 'Answer must be at least 20 characters';
            errorElement.classList.remove('d-none');
            return;
        }

        if (!questionId || questionId === 0) {
            errorElement.textContent = 'Invalid question ID';
            errorElement.classList.remove('d-none');
            console.error('Question ID is missing or invalid:', questionId);
            return;
        }

        console.log('Submitting answer:', { body: body.substring(0, 50) + '...', questionId });

        try {
            const response = await fetch('/api/answers', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getToken()
                },
                body: JSON.stringify({ 
                    body: body, 
                    questionId: questionId 
                })
            });

            console.log('Response status:', response.status);

            if (response.ok) {
                console.log('Answer posted successfully, reloading page...');
                location.reload();
            } else if (response.status === 401) {
                console.error('Unauthorized - redirecting to login');
                window.location.href = '/Login';
            } else {
                const data = await response.json().catch(() => ({ message: 'Failed to post answer' }));
                console.error('Error response:', data);
                errorElement.textContent = data.message || 'Failed to post answer';
                errorElement.classList.remove('d-none');
            }
        } catch (error) {
            console.error('Exception posting answer:', error);
            errorElement.textContent = 'An error occurred. Please try again.';
            errorElement.classList.remove('d-none');
        }
    };

    window.addComment = async function (questionId, answerId) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        let body;
        if (questionId) {
            body = document.getElementById('question-comment-text').value;
        } else {
            body = document.getElementById('answer-' + answerId + '-comment-text').value;
        }

        if (body.length < 5) {
            alert('Comment must be at least 5 characters');
            return;
        }

        try {
            const response = await fetch('/api/comments', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getToken()
                },
                body: JSON.stringify({ body, questionId, answerId })
            });

            if (response.ok) {
                location.reload();
            } else if (response.status === 401) {
                window.location.href = '/Login';
            }
        } catch (error) {
            console.error('Error posting comment:', error);
        }
    };

    function initializeCommentControls() {
        const token = getToken();
        const currentUserId = Number(localStorage.getItem('userId'));
        if (!token) return;

        document.querySelectorAll('[id^="editComment-"]').forEach(btn => {
            const owner = Number(btn.getAttribute('data-owner'));
            const commentId = btn.id.split('-')[1];
            const del = document.getElementById('deleteComment-' + commentId);
            if (owner === currentUserId) {
                btn.style.display = 'inline-block';
                if (del) del.style.display = 'inline-block';
            } else {
                btn.style.display = 'none';
                if (del) del.style.display = 'none';
            }
        });
    }

    window.deleteComment = async function (commentId) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        if (!confirm('Delete this comment?')) return;

        try {
            const response = await fetch('/api/comments/' + commentId, {
                method: 'DELETE',
                headers: {
                    'Authorization': 'Bearer ' + getToken()
                }
            });

            if (response.ok) {
                const node = document.querySelector('[data-comment-id="' + commentId + '"]');
                if (node) node.remove();
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                const data = await response.json();
                alert(data.message || 'Failed to delete comment');
            }
        } catch (err) {
            console.error(err);
            alert('An error occurred. Please try again.');
        }
    };

    // Show edit form for comment
    window.showEditCommentForm = function (commentId) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        // Hide the comment display and show the edit form
        const displayDiv = document.querySelector('.comment-display-' + commentId);
        const editForm = document.querySelector('.comment-edit-form-' + commentId);
        
        if (displayDiv) displayDiv.style.display = 'none';
        if (editForm) editForm.style.display = 'block';
    };

    // Cancel editing comment
    window.cancelEditComment = function (commentId) {
        // Show the comment display and hide the edit form
        const displayDiv = document.querySelector('.comment-display-' + commentId);
        const editForm = document.querySelector('.comment-edit-form-' + commentId);
        
        if (displayDiv) displayDiv.style.display = 'flex';
        if (editForm) editForm.style.display = 'none';
        
        // Reset the input value to original
        const input = document.getElementById('edit-comment-text-' + commentId);
        const originalText = document.querySelector('.comment-body-' + commentId)?.innerText || '';
        if (input) {
            // Extract just the comment body (before the username)
            const bodyMatch = originalText.match(/^(.+?) - /);
            if (bodyMatch) {
                input.value = bodyMatch[1];
            }
        }
    };

    // Update comment with new text
    window.updateComment = async function (commentId) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        const input = document.getElementById('edit-comment-text-' + commentId);
        if (!input) return;

        const newBody = input.value.trim();
        
        if (newBody.length < 5) {
            alert('Comment must be at least 5 characters');
            return;
        }

        try {
            const response = await fetch('/api/comments/' + commentId, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getToken()
                },
                body: JSON.stringify({ body: newBody })
            });

            if (response.ok) {
                location.reload();
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                const data = await response.json();
                alert(data.message || 'Failed to update comment');
            }
        } catch (err) {
            console.error(err);
            alert('An error occurred. Please try again.');
        }
    };

    // Initialize on DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        console.log('Initializing question details page...');
        console.log('Page data:', pageData);
        initializeOwnershipControls();
        attachAnswerDeleteHandlers();
        attachQuestionDeleteHandler();
        initializeAnswerForm();
        initializeCommentControls();
    });
})();