// JWT Token Management
function getToken() {
    return localStorage.getItem('jwtToken');
}

function setToken(token) {
    localStorage.setItem('jwtToken', token);
    updateAuthUI();
}

function removeToken() {
    localStorage.removeItem('jwtToken');
    localStorage.removeItem('username');
    localStorage.removeItem('userId');
    updateAuthUI();
}

function logout() {
    removeToken();
    window.location.href = '/';
}

function updateAuthUI() {
    const token = getToken();
    const username = localStorage.getItem('username');

    if (token) {
        document.getElementById('loginNav').classList.add('d-none');
        document.getElementById('registerNav').classList.add('d-none');
        document.getElementById('userNav').classList.remove('d-none');
        document.getElementById('logoutNav').classList.remove('d-none');
        document.getElementById('askQuestionNav').classList.remove('d-none');
        document.getElementById('usernameDisplay').textContent = username || 'User';
    } else {
        document.getElementById('loginNav').classList.remove('d-none');
        document.getElementById('registerNav').classList.remove('d-none');
        document.getElementById('userNav').classList.add('d-none');
        document.getElementById('logoutNav').classList.add('d-none');
        document.getElementById('askQuestionNav').classList.add('d-none');
    }
}

// Call on page load
document.addEventListener('DOMContentLoaded', updateAuthUI);