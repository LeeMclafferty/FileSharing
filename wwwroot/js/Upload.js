document.addEventListener('DOMContentLoaded', () => {
    const fileInput = document.getElementById('file-input');
    const dropArea = document.getElementById('drop-area');
    const browseButton = document.getElementById('browse-button');
    const fileNameDisplay = document.getElementById('file-name');

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

    // Handle form submission
    const uploadForm = document.getElementById('upload-form');
    uploadForm.addEventListener('submit', (e) => {
        e.preventDefault();
        const formData = new FormData();
        const file = fileInput.files[0];
        if (file) {
            formData.append('file', file);

            fetch(`/api/files/upload`, {
                method: 'POST',
                body: formData,
            })
                .then(response => {
                    if (response.ok) {
                        return response.json();
                    } else {
                        return response.text().then(text => { throw new Error(text); });
                    }
                })
                .then(data => {
                    alert('File uploaded successfully');
                    fileNameDisplay.textContent = '';
                    fileInput.value = '';
                })
                .catch(error => {
                    console.error('Error uploading file:', error);
                    alert('Failed to upload file');
                });
        } else {
            alert('No file selected');
        }
    });
});
