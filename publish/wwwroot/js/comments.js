import connection from './signalr-init.js';

document.addEventListener('DOMContentLoaded', () => {
    const commentForm = document.getElementById('commentForm');
    const commentBody = document.getElementById('commentBody');
    const submitBtn = document.getElementById('submitCommentBtn');
    const spinner = document.getElementById('commentSpinner');
    const icon = document.getElementById('commentIcon');
    const isInternalNote = document.getElementById('isInternalNote');
    const commentThread = document.getElementById('commentThread');
    
    // Extract request ID from URL - assuming URL structure like /ServiceRequests/Details/5
    const urlParts = window.location.pathname.split('/');
    const requestId = urlParts[urlParts.length - 1];

    if (commentForm) {
        commentForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const bodyText = commentBody.value.trim();
            if (!bodyText) return;

            const isInternal = isInternalNote ? isInternalNote.checked : false;

            // UI Loading state
            submitBtn.disabled = true;
            spinner.classList.remove('d-none');
            icon.classList.add('d-none');

            try {
                const formData = new FormData();
                formData.append('body', bodyText);
                formData.append('isInternal', isInternal);

                const response = await fetch(`/ServiceRequest/${requestId}/Comment`, {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    const data = await response.json();
                    
                    // Prepend comment card dynamically
                    prependCommentCard(data);
                    
                    // Reset form
                    commentBody.value = '';
                    if (isInternalNote) isInternalNote.checked = false;
                    
                    // Remove "No comments" message if present
                    const noCommentsMsg = document.getElementById('no-comments-msg');
                    if (noCommentsMsg) noCommentsMsg.remove();
                } else {
                    const errorText = await response.text();
                    console.error('Failed to submit comment', response.status, errorText);
                    alert(`Error submitting comment (${response.status}): ${errorText}`);
                }
            } catch (err) {
                console.error('Error in fetch:', err);
                alert('Error in fetch: ' + err.message);
            } finally {
                // Reset UI loading state
                submitBtn.disabled = false;
                spinner.classList.add('d-none');
                icon.classList.remove('d-none');
            }
        });
    }

    // SignalR listener for incoming comments
    connection.on("ReceiveComment", (payload) => {
        // Only show toast and append if it's for the current request
        // Alternatively, since payload doesn't strictly have requestId, just requestNumber
        // Let's check if we are on the same request page based on title or URL
        
        const pageTitle = document.querySelector('h4')?.textContent || '';
        if (pageTitle.includes(payload.requestNumber)) {
             // Since we modified NotificationService to include body, we can just pass the payload
             prependCommentCard(payload);
        }
        
        showToast(`New reply on Request #${payload.requestNumber} by ${payload.authorName}`);
    });

    // SignalR listener for status updates
    connection.on("ReceiveStatusUpdate", (payload) => {
        const pageTitle = document.querySelector('h4')?.textContent || '';
        
        // Show toast notification globally
        showToast(`Request #${payload.requestNumber}: ${payload.message}`);

        if (pageTitle.includes(payload.requestNumber)) {
             // Update status badge
             const badge = document.getElementById('status-badge');
             if (badge) {
                 badge.textContent = payload.newStatus;
                 
                 // Update badge class
                 badge.className = 'badge fs-6'; // Reset base classes
                 switch (payload.newStatus) {
                     case 'Pending': badge.classList.add('bg-primary'); break;
                     case 'InProgress': badge.classList.add('bg-warning', 'text-dark'); break;
                     case 'OnHold': badge.classList.add('bg-secondary'); break;
                     case 'Resolved': badge.classList.add('bg-success'); break;
                     case 'Closed': badge.classList.add('bg-dark'); break;
                     default: badge.classList.add('bg-light', 'text-dark'); break;
                 }
             }

             // Check if we need to show the Feedback button
             if (payload.newStatus === 'Resolved' || payload.newStatus === 'Closed') {
                 const actionsContainer = document.getElementById('sidebar-actions-container');
                 if (actionsContainer && !document.querySelector('a[href*="/Feedback/Create"]')) {
                     // Check if user is Employee (hacky way: if there are no technician/admin buttons, we are employee)
                     // But even simpler: if there is no Provide Feedback button, and there's no Edit Request button (unless we are admin)
                     // Actually, just doing window.location.reload() would give the true "persistent refresh" feel,
                     // but the user said "persistent refresh on status", maybe they mean dynamic updates.
                     // I will just add the button dynamically if it's not there.
                     
                     const isEmployee = !document.querySelector('a[href*="Assign"]'); // Assign button only for admins
                     if (isEmployee) {
                         const feedbackBtn = document.createElement('a');
                         feedbackBtn.href = `/Feedback/Create?requestId=${requestId}`;
                         feedbackBtn.className = 'btn btn-primary mt-2';
                         feedbackBtn.textContent = 'Provide Feedback';
                         actionsContainer.insertBefore(feedbackBtn, actionsContainer.lastElementChild); // Insert before "Back to List"
                     }
                 }
             }
        }
    });

    function prependCommentCard(data) {
        const thread = document.getElementById('comment-thread');
        if (!thread) return;

        const isInternal = data.isInternal;
        const cardClass = isInternal ? "card mb-3 bg-warning bg-opacity-10" : "card mb-3";
        const badgeHtml = isInternal ? '<span class="badge bg-warning text-dark me-2">Internal note</span>' : '';
        const safeBody = data.body ? data.body.replace(/\n/g, "<br>") : "New comment.";
        const author = data.authorName || "Unknown User";
        const time = data.createdAt || "Just now";

        const cardHtml = `
            <div class="${cardClass}">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <h6 class="card-subtitle text-muted">
                            <i class="bi bi-person-circle me-1"></i> ${escapeHtml(author)}
                        </h6>
                        <small class="text-muted">
                            ${badgeHtml}
                            <i class="bi bi-clock me-1"></i> ${time}
                        </small>
                    </div>
                    <p class="card-text">${safeBody}</p>
                </div>
            </div>
        `;

        thread.insertAdjacentHTML('afterbegin', cardHtml);
    }

    function escapeHtml(unsafe) {
        return (unsafe || '').replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function showToast(message) {
        // Create a Bootstrap toast dynamically
        const toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            const container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            document.body.appendChild(container);
        }

        const toastHtml = `
            <div class="toast align-items-center text-white bg-primary border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        ${escapeHtml(message)}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
        
        const containerElem = document.getElementById('toast-container');
        containerElem.insertAdjacentHTML('beforeend', toastHtml);
        
        const toastEl = containerElem.lastElementChild;
        const bsToast = new bootstrap.Toast(toastEl, { delay: 5000 });
        bsToast.show();
        
        toastEl.addEventListener('hidden.bs.toast', () => {
            toastEl.remove();
        });
    }
});
