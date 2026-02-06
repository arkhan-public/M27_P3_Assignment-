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

        const body = document.getElementById('answerBody').value;
        const questionId = pageData.questionId || 0;

        if (body.length < 20) {
            document.getElementById('answerError').textContent = 'Answer must be at least 20 characters';
            document.getElementById('answerError').classList.remove('d-none');
            return;
        }

        try {
            const response = await fetch('/api/answers', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + getToken()
                },
                body: JSON.stringify({ body, questionId })
            });

            if (response.ok) {
                location.reload();
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                const data = await response.json();
                document.getElementById('answerError').textContent = data.message || 'Failed to post answer';
                document.getElementById('answerError').classList.remove('d-none');
            }
        } catch (error) {
            document.getElementById('answerError').textContent = 'An error occurred. Please try again.';
            document.getElementById('answerError').classList.remove('d-none');
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

    window.editComment = async function (commentId) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        const container = document.querySelector('[data-comment-id="' + commentId + '"]');
        if (!container) return;
        const currentText = container.querySelector('small')?.innerText || '';
        const newBody = prompt('Edit your comment', currentText);
        if (newBody === null) return;
        if (newBody.trim().length < 5) {
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
        initializeOwnershipControls();
        attachAnswerDeleteHandlers();
        attachQuestionDeleteHandler();
        initializeAnswerForm();
        initializeCommentControls();
    });
})();