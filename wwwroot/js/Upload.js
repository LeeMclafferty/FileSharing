document.addEventListener('DOMContentLoaded', () => {
    const fileInput = document.getElementById('file-input');
    const dropArea = document.getElementById('drop-area');
    const browseButton = document.getElementById('browse-button');
    const fileNameDisplay = document.getElementById('file-name');
    const uploadForm = document.getElementById('upload-form');

    // Handle browse button click
    browseButton.addEventListener('click', () => {
        fileInput.click();
    });

    // Handle file input change
    fileInput.addEventListener('change', () => {
        const file = fileInput.files[0];
        if (file) {
            fileNameDisplay.textContent = `Selected file: ${file.name}`;
        }
    });

    // Prevent default drag behaviors
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, preventDefaults, false);
    });

    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    // Highlight drop area when item is dragged over it
    ['dragenter', 'dragover'].forEach(eventName => {
        dropArea.addEventListener(eventName, () => {
            dropArea.classList.add('dragover');
        }, false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropArea.addEventListener(eventName, () => {
            dropArea.classList.remove('dragover');
        }, false);
    });

    // Handle dropped files
    dropArea.addEventListener('drop', (e) => {
        const dt = e.dataTransfer;
        const file = dt.files[0];
        fileInput.files = dt.files;
        fileNameDisplay.textContent = `Selected file: ${file.name}`;
    });

    // BEING HANDLED ON BACK END
    /*
    // Make upload call
    function uploadFile(formData) {
        return fetch(`/api/files/upload`, {
            method: 'POST',
            body: formData,
        })
            .then(response => {
                if (response.ok) {
                    return response.json();
                } else {
                    return response.text().then(text => { throw new Error(text); });
                }
            });
    }
    
    function sendDownloadEmail(recipientEmail, downloadUrl) {
        const emailData = {
            recipientEmail: recipientEmail,
            downloadUri: downloadUrl
        };

        return fetch(`/api/files/send`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(emailData)
        });
    }

    // Handle form submission logic
    function handleFormSubmit() {
        const file = fileInput.files[0];
        const recipientEmail = document.getElementById('email-input').value;

        if (file && recipientEmail) {
            const formData = new FormData();
            formData.append('file', file);

            uploadFile(formData)
                .then(data => {
                    const downloadUrl = data.downloadUrl;  // Assume the server returns the download URL

                    return sendDownloadEmail(recipientEmail, downloadUrl);
                })
                .then(() => {
                    alert('File uploaded and email sent successfully');
                    clearForm();
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('Failed to upload file or send email');
                });
        } else {
            alert('No file selected or no email provided');
        }
    }
    */


    // Clear form after successful upload
    function clearForm() {
        fileNameDisplay.textContent = '';
        fileInput.value = '';
    }
});
