const API_BASE = '/api/user';

// Load users when page loads
document.addEventListener('DOMContentLoaded', function() {
    loadUsers();
});

// Handle form submission
document.getElementById('createUserForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    
    try {
        const response = await fetch(API_BASE, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                username: username,
                password: password
            })
        });

        if (response.ok) {
            const newUser = await response.json();
            showMessage('User created successfully!', 'success');
            document.getElementById('createUserForm').reset();
            loadUsers(); // Refresh the users list
        } else {
            const errorText = await response.text();
            showMessage(`Error: ${errorText}`, 'error');
        }
    } catch (error) {
        showMessage(`Network error: ${error.message}`, 'error');
    }
});

// Load and display users
async function loadUsers() {
    try {
        const response = await fetch(API_BASE);
        
        if (response.ok) {
            const users = await response.json();
            displayUsers(users);
        } else {
            document.getElementById('usersList').innerHTML = '<p class="error">Failed to load users</p>';
        }
    } catch (error) {
        document.getElementById('usersList').innerHTML = `<p class="error">Error: ${error.message}</p>`;
    }
}

// Display users in the UI
function displayUsers(users) {
    const usersList = document.getElementById('usersList');
    
    if (users.length === 0) {
        usersList.innerHTML = '<p>No users found.</p>';
        return;
    }

    let html = '';
    users.forEach(user => {
        html += `
            <div class="user-item">
                <strong>Username:</strong> ${user.username || 'N/A'}<br>
                <strong>ID:</strong> ${user.id}<br>
                <strong>Created:</strong> ${new Date(user.createdAt).toLocaleString()}<br>
                <button onclick="deleteUser('${user.id}')" style="background-color: #dc3545;">Delete</button>
            </div>
        `;
    });
    
    usersList.innerHTML = html;
}

// Delete user
async function deleteUser(userId) {
    if (!confirm('Are you sure you want to delete this user?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/${userId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showMessage('User deleted successfully!', 'success');
            loadUsers(); // Refresh the users list
        } else {
            showMessage('Failed to delete user', 'error');
        }
    } catch (error) {
        showMessage(`Error: ${error.message}`, 'error');
    }
}

// Show success/error messages
function showMessage(text, type) {
    const messageDiv = document.getElementById('message');
    messageDiv.textContent = text;
    messageDiv.className = type;
    
    // Clear message after 5 seconds
    setTimeout(() => {
        messageDiv.textContent = '';
        messageDiv.className = '';
    }, 5000);
}