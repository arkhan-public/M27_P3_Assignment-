// Question Details Page JavaScript
(function () {
    'use strict';

    // Page Data (will be set from Razor Page)
    let pageData = window.questionDetailsData || {};

    // Store current vote states
    let userVotes = {
        question: null, // null, 1, or -1
        answers: {} // answerId: null, 1, or -1
    };

    // Load user's current votes
    async function loadUserVotes() {
        const token = getToken();
        if (!token) return;

        try {
            // Load question vote status
            const questionId = pageData.questionId;
            if (questionId) {
                const response = await fetch(`/api/vote/status?questionId=${questionId}`, {
                    headers: {
                        'Authorization': 'Bearer ' + token
                    }
                });
                if (response.ok) {
                    const data = await response.json();
                    userVotes.question = data.hasVoted ? data.voteValue : null;
                    updateVoteButtons('question', questionId, userVotes.question);
                }
            }

            // Load answer vote statuses
            document.querySelectorAll('[data-answer-id]').forEach(async (elem) => {
                const answerId = elem.getAttribute('data-answer-id');
                const response = await fetch(`/api/vote/status?answerId=${answerId}`, {
                    headers: {
                        'Authorization': 'Bearer ' + token
                    }
                });
                if (response.ok) {
                    const data = await response.json();
                    userVotes.answers[answerId] = data.hasVoted ? data.voteValue : null;
                    updateVoteButtons('answer', answerId, userVotes.answers[answerId]);
                }
            });
        } catch (error) {
            console.error('Error loading vote statuses:', error);
        }
    }

    // Update vote button states
    // Updated: Highlight active vote, but DON'T disable it (allow toggling)
    function updateVoteButtons(type, id, currentVote) {
        let upvoteBtn, downvoteBtn;
        
        if (type === 'question') {
            upvoteBtn = document.getElementById('question-upvote');
            downvoteBtn = document.getElementById('question-downvote');
        } else {
            upvoteBtn = document.getElementById(`answer-${id}-upvote`);
            downvoteBtn = document.getElementById(`answer-${id}-downvote`);
        }

        if (!upvoteBtn || !downvoteBtn) return;

        // Reset styles - Keep buttons ENABLED
        upvoteBtn.classList.remove('active-vote');
        downvoteBtn.classList.remove('active-vote');

        if (currentVote === 1) {
            // Highlight upvote button (but keep it clickable to remove vote)
            upvoteBtn.classList.add('active-vote');
        } else if (currentVote === -1) {
            // Highlight downvote button (but keep it clickable to remove vote)
            downvoteBtn.classList.add('active-vote');
        }
    }

    // Initialize vote button states based on ownership
    function initializeVoteButtons() {
        const token = getToken();
        if (!token) return;

        const currentUserId = Number(localStorage.getItem('userId'));
        if (!currentUserId) return;

        // Disable voting buttons ONLY for owned content
        document.querySelectorAll('.vote-btn').forEach(btn => {
            const ownerId = Number(btn.getAttribute('data-owner'));
            
            if (ownerId === currentUserId) {
                btn.classList.add('disabled-vote');
                btn.style.opacity = '0.4';
                btn.style.cursor = 'not-allowed';
                btn.style.pointerEvents = 'none';
            }
        });

        // Load and apply user's current votes
        loadUserVotes();
    }

    function initializeOwnershipControls() {
        const token = getToken();
        const currentUserId = Number(localStorage.getItem('userId'));
        const questionOwnerId = pageData.questionOwnerId || 0;

        if (token && currentUserId === questionOwnerId) {
            const editBtn = document.getElementById('editQuestionBtn');
            const deleteBtn = document.getElementById('deleteQuestionBtn');
            if (editBtn) editBtn.style.display = 'inline-block';
            if (deleteBtn) deleteBtn.style.display = 'inline-block';

            document.querySelectorAll('[id^="accept-"]').forEach(btn => btn.style.display = 'inline-block');
        }

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
            } else {
                const data = await response.json();
                if (data.message) {
                    alert(data.message);
                }
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

        errorElement.classList.add('d-none');
        errorElement.textContent = '';

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

            if (response.ok) {
                location.reload();
            } else if (response.status === 401) {
                window.location.href = '/Login';
            } else {
                const data = await response.json().catch(() => ({ message: 'Failed to post answer' }));
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

    window.showEditCommentForm = function (commentId) {
        if (!getToken()) {
            window.location.href = '/Login';
            return;
        }

        const displayDiv = document.querySelector('.comment-display-' + commentId);
        const editForm = document.querySelector('.comment-edit-form-' + commentId);
        
        if (displayDiv) displayDiv.style.display = 'none';
        if (editForm) editForm.style.display = 'block';
    };

    window.cancelEditComment = function (commentId) {
        const displayDiv = document.querySelector('.comment-display-' + commentId);
        const editForm = document.querySelector('.comment-edit-form-' + commentId);
        
        if (displayDiv) displayDiv.style.display = 'flex';
        if (editForm) editForm.style.display = 'none';
        
        const input = document.getElementById('edit-comment-text-' + commentId);
        const originalText = document.querySelector('.comment-body-' + commentId)?.innerText || '';
        if (input) {
            const bodyMatch = originalText.match(/^(.+?) - /);
            if (bodyMatch) {
                input.value = bodyMatch[1];
            }
        }
    };

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
        initializeVoteButtons();
        initializeOwnershipControls();
        attachAnswerDeleteHandlers();
        attachQuestionDeleteHandler();
        initializeAnswerForm();
        initializeCommentControls();
    });
})();