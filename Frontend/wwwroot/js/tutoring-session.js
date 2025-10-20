// Tutoring Session Room JavaScript Functions

let localStream = null;
let peerConnection = null;
let isVideoEnabled = true;
let isAudioEnabled = true;
let isScreenSharing = false;
let otherUserConnectionId = null;
let isInitiator = false;

// Initialize media streams
window.initializeMedia = async function() {
    try {
        console.log('[TutoringSession] Starting media initialization...');
        
        // Clean up any existing media first
        await window.cleanupMedia();
        
        // Check if getUserMedia is supported
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            throw new Error('getUserMedia is not supported in this browser');
        }

        console.log('[TutoringSession] Requesting user media...');
        
        // Get user media with error handling for device busy
        try {
            localStream = await navigator.mediaDevices.getUserMedia({
                video: {
                    width: { ideal: 1280 },
                    height: { ideal: 720 },
                    facingMode: 'user'
                },
                audio: true
            });
        } catch (mediaError) {
            console.error('[TutoringSession] Error getting user media:', mediaError);
            
            // If device is busy, wait a bit and try again
            if (mediaError.name === 'NotReadableError' || mediaError.name === 'NotAllowedError') {
                console.log('[TutoringSession] Device busy, waiting 2 seconds before retry...');
                await new Promise(resolve => setTimeout(resolve, 2000));
                
                localStream = await navigator.mediaDevices.getUserMedia({
                    video: {
                        width: { ideal: 1280 },
                        height: { ideal: 720 },
                        facingMode: 'user'
                    },
                    audio: true
                });
            } else {
                throw mediaError;
            }
        }

        console.log('[TutoringSession] User media obtained:', localStream);

        // Wait for DOM to be ready and retry if element not found
        let localVideo = document.getElementById('localVideo');
        let retryCount = 0;
        const maxRetries = 20;
        
        while (!localVideo && retryCount < maxRetries) {
            console.log(`[TutoringSession] localVideo element not found, retrying... (${retryCount + 1}/${maxRetries})`);
            await new Promise(resolve => setTimeout(resolve, 200)); // Wait 200ms
            localVideo = document.getElementById('localVideo');
            retryCount++;
        }

        if (localVideo) {
            console.log('[TutoringSession] Setting video source...');
            localVideo.srcObject = localStream;
            
            // Add event listeners to check if video is playing
            localVideo.onloadedmetadata = () => {
                console.log('[TutoringSession] Video metadata loaded');
                localVideo.play().then(() => {
                    console.log('[TutoringSession] Video started playing');
                    // Hide placeholder when video starts
                    const placeholder = document.getElementById('videoPlaceholder');
                    if (placeholder) {
                        placeholder.style.display = 'none';
                    }
                }).catch(err => {
                    console.error('[TutoringSession] Error playing video:', err);
                });
            };

            localVideo.oncanplay = () => {
                console.log('[TutoringSession] Video can play');
                // Hide placeholder when video can play
                const placeholder = document.getElementById('videoPlaceholder');
                if (placeholder) {
                    placeholder.style.display = 'none';
                }
            };

            localVideo.onerror = (error) => {
                console.error('[TutoringSession] Video error:', error);
            };

            localVideo.onplaying = () => {
                console.log('[TutoringSession] Local video is playing');
                // Ensure local video is visible
                localVideo.style.display = 'block';
            };
        } else {
            console.error('[TutoringSession] localVideo element not found after retries');
            
            // Show error in placeholder
            const placeholder = document.getElementById('videoPlaceholder');
            if (placeholder) {
                const placeholderContent = placeholder.querySelector('.placeholder-content');
                if (placeholderContent) {
                    const icon = placeholderContent.querySelector('.placeholder-icon');
                    const text = placeholderContent.querySelector('p');
                    
                    if (icon) icon.textContent = '❌';
                    if (text) text.textContent = 'Video element not found';
                }
            }
        }

        // Initialize WebRTC peer connection
        await initializePeerConnection();

        console.log('[TutoringSession] Media initialized successfully');
    } catch (error) {
        console.error('[TutoringSession] Error initializing media:', error);
        console.error('[TutoringSession] Error details:', error.message);
        
        // Show error in placeholder
        const placeholder = document.getElementById('videoPlaceholder');
        if (placeholder) {
            const placeholderContent = placeholder.querySelector('.placeholder-content');
            if (placeholderContent) {
                const icon = placeholderContent.querySelector('.placeholder-icon');
                const text = placeholderContent.querySelector('p');
                
                if (icon) icon.textContent = '❌';
                
                if (error.name === 'NotAllowedError') {
                    if (text) text.textContent = 'Camera access denied';
                } else if (error.name === 'NotFoundError') {
                    if (text) text.textContent = 'No camera found';
                } else if (error.name === 'NotSupportedError') {
                    if (text) text.textContent = 'Camera not supported';
                } else {
                    if (text) text.textContent = 'Camera error: ' + error.message;
                }
            }
        }

        // Show user-friendly error message
        if (error.name === 'NotAllowedError') {
            alert('Camera access denied. Please allow camera access and refresh the page.');
        } else if (error.name === 'NotFoundError') {
            alert('No camera found. Please connect a camera and refresh the page.');
        } else if (error.name === 'NotSupportedError') {
            alert('Camera not supported in this browser. Please use a modern browser.');
        } else {
            alert('Error accessing camera: ' + error.message);
        }
    }
};

// Initialize WebRTC peer connection
async function initializePeerConnection() {
    const configuration = {
        iceServers: [
            { urls: 'stun:stun.l.google.com:19302' },
            { urls: 'stun:stun1.l.google.com:19302' }
        ]
    };

    peerConnection = new RTCPeerConnection(configuration);

    // Add local stream to peer connection
    if (localStream) {
        localStream.getTracks().forEach(track => {
            peerConnection.addTrack(track, localStream);
        });
    }

    // Handle incoming tracks
    peerConnection.ontrack = (event) => {
        console.log('[TutoringSession] Received remote stream');
        const remoteVideo = document.getElementById('remoteVideo');
        if (remoteVideo && event.streams[0]) {
            remoteVideo.srcObject = event.streams[0];
            remoteVideo.style.display = 'block';
            
            // Remove any demo placeholder content
            const demoPlaceholder = remoteVideo.querySelector('.demo-placeholder');
            if (demoPlaceholder) {
                demoPlaceholder.remove();
            }
            
            // Reset video styling
            remoteVideo.style.backgroundColor = 'transparent';
            remoteVideo.style.border = 'none';
            
            // Add event listener for when remote video starts playing
            remoteVideo.onplaying = () => {
                console.log('[TutoringSession] Remote video is playing');
            };
            
            console.log('[TutoringSession] Remote video stream started');
        }
    };

    // Handle ICE candidates
    peerConnection.onicecandidate = (event) => {
        if (event.candidate && otherUserConnectionId) {
            console.log('[TutoringSession] Sending ICE candidate');
            // Send ICE candidate via SignalR
            if (blazorComponent) {
                blazorComponent.invokeMethodAsync('SendIceCandidate', otherUserConnectionId, event.candidate);
            }
        }
    };

    // Handle connection state changes
    peerConnection.onconnectionstatechange = () => {
        console.log('[TutoringSession] Connection state:', peerConnection.connectionState);
        if (peerConnection.connectionState === 'connected') {
            console.log('[TutoringSession] WebRTC connection established successfully!');
        } else if (peerConnection.connectionState === 'failed') {
            console.error('[TutoringSession] WebRTC connection failed');
        }
    };

    // Handle ICE connection state changes
    peerConnection.oniceconnectionstatechange = () => {
        console.log('[TutoringSession] ICE connection state:', peerConnection.iceConnectionState);
        if (peerConnection.iceConnectionState === 'connected') {
            console.log('[TutoringSession] ICE connection established!');
        } else if (peerConnection.iceConnectionState === 'failed') {
            console.error('[TutoringSession] ICE connection failed');
        }
    };

    // Handle ICE gathering state changes
    peerConnection.onicegatheringstatechange = () => {
        console.log('[TutoringSession] ICE gathering state:', peerConnection.iceGatheringState);
    };
}

// Toggle video
window.toggleVideo = async function(enabled) {
    isVideoEnabled = enabled;
    if (localStream) {
        const videoTrack = localStream.getVideoTracks()[0];
        if (videoTrack) {
            videoTrack.enabled = enabled;
        }
    }
};

// Toggle audio
window.toggleAudio = async function(enabled) {
    isAudioEnabled = enabled;
    if (localStream) {
        const audioTrack = localStream.getAudioTracks()[0];
        if (audioTrack) {
            audioTrack.enabled = enabled;
        }
    }
};

// Toggle screen sharing
window.toggleScreenShare = async function(enabled) {
    try {
        if (enabled && !isScreenSharing) {
            // Start screen sharing
            const screenStream = await navigator.mediaDevices.getDisplayMedia({
                video: true,
                audio: true
            });

            // Replace video track in peer connection
            const videoTrack = screenStream.getVideoTracks()[0];
            const sender = peerConnection.getSenders().find(s => s.track && s.track.kind === 'video');
            
            if (sender) {
                await sender.replaceTrack(videoTrack);
            }

            // Update local video
            const localVideo = document.getElementById('localVideo');
            if (localVideo) {
                localVideo.srcObject = screenStream;
            }

            isScreenSharing = true;

            // Handle screen sharing end
            videoTrack.onended = () => {
                stopScreenShare();
            };
        } else if (!enabled && isScreenSharing) {
            await stopScreenShare();
        }
    } catch (error) {
        console.error('Error toggling screen share:', error);
    }
};

// Stop screen sharing
async function stopScreenShare() {
    try {
        // Get back to camera
        const cameraStream = await navigator.mediaDevices.getUserMedia({
            video: true,
            audio: false
        });

        const videoTrack = cameraStream.getVideoTracks()[0];
        const sender = peerConnection.getSenders().find(s => s.track && s.track.kind === 'video');
        
        if (sender) {
            await sender.replaceTrack(videoTrack);
        }

        // Update local video
        const localVideo = document.getElementById('localVideo');
        if (localVideo) {
            localVideo.srcObject = cameraStream;
        }

        isScreenSharing = false;
    } catch (error) {
        console.error('Error stopping screen share:', error);
    }
}

// WebRTC connection setup
window.setOtherUserConnectionId = function(connectionId) {
    console.log('[TutoringSession] Setting up WebRTC connection with:', connectionId);
    otherUserConnectionId = connectionId;
    
    // If we're the first user to join, we'll be the initiator
    // The second user will receive our offer
    if (!isInitiator) {
        isInitiator = true;
        console.log('[TutoringSession] Setting as initiator, will create offer when ready');
    }
};

window.clearOtherUserConnectionId = function() {
    console.log('[TutoringSession] Other user disconnected');
    otherUserConnectionId = null;
    isInitiator = false;
    
    // Hide remote video
    const remoteVideo = document.getElementById('remoteVideo');
    if (remoteVideo) {
        remoteVideo.style.display = 'none';
        remoteVideo.srcObject = null;
    }
};

// Simulate remote connection for demo purposes
function simulateRemoteConnection() {
    console.log('[TutoringSession] Simulating remote video connection...');
    
    // Create a simple remote video element for demo
    const remoteVideo = document.getElementById('remoteVideo');
    if (remoteVideo) {
        // For demo purposes, we'll show a placeholder
        // In a real implementation, this would be the actual remote video stream
        remoteVideo.style.display = 'block';
        remoteVideo.style.backgroundColor = '#f0f0f0';
        remoteVideo.style.border = '2px solid #007bff';
        
        // Add some demo content
        if (!remoteVideo.querySelector('.demo-placeholder')) {
            const placeholder = document.createElement('div');
            placeholder.className = 'demo-placeholder';
            placeholder.style.cssText = `
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                text-align: center;
                color: #666;
                font-size: 14px;
            `;
            placeholder.innerHTML = '📹 Remote Video<br/>Demo Mode';
            remoteVideo.appendChild(placeholder);
        }
        
        console.log('[TutoringSession] Remote video placeholder displayed');
    }
}

// WebRTC signaling functions
async function createOffer() {
    if (!peerConnection || !otherUserConnectionId) {
        console.log('[TutoringSession] Cannot create offer: peerConnection or otherUserConnectionId not available');
        return;
    }
    
    try {
        console.log('[TutoringSession] Creating offer...');
        const offer = await peerConnection.createOffer();
        await peerConnection.setLocalDescription(offer);
        
        console.log('[TutoringSession] Sending offer to', otherUserConnectionId);
        if (blazorComponent) {
            blazorComponent.invokeMethodAsync('SendOffer', otherUserConnectionId, offer);
        }
    } catch (error) {
        console.error('[TutoringSession] Error creating offer:', error);
    }
}

window.handleReceiveOffer = async function(fromConnectionId, offer) {
    if (!peerConnection) {
        console.log('[TutoringSession] No peer connection available for offer');
        return;
    }
    
    try {
        console.log('[TutoringSession] Received offer, creating answer...');
        otherUserConnectionId = fromConnectionId; // remember who to answer
        await peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
        
        const answer = await peerConnection.createAnswer();
        await peerConnection.setLocalDescription(answer);
        
        console.log('[TutoringSession] Sending answer to', otherUserConnectionId);
        if (blazorComponent && otherUserConnectionId) {
            blazorComponent.invokeMethodAsync('SendAnswer', otherUserConnectionId, answer);
        }
    } catch (error) {
        console.error('[TutoringSession] Error handling offer:', error);
    }
};

window.handleReceiveAnswer = async function(fromConnectionId, answer) {
    if (!peerConnection) {
        console.log('[TutoringSession] No peer connection available for answer');
        return;
    }
    
    try {
        console.log('[TutoringSession] Received answer, setting remote description...');
        otherUserConnectionId = otherUserConnectionId || fromConnectionId;
        await peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
    } catch (error) {
        console.error('[TutoringSession] Error handling answer:', error);
    }
};

window.handleReceiveIceCandidate = async function(fromConnectionId, candidate) {
    if (!peerConnection) {
        console.log('[TutoringSession] No peer connection available for ICE candidate');
        return;
    }
    
    try {
        console.log('[TutoringSession] Received ICE candidate, adding to peer connection...');
        otherUserConnectionId = otherUserConnectionId || fromConnectionId;
        await peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
    } catch (error) {
        console.error('[TutoringSession] Error handling ICE candidate:', error);
    }
};

// SignalR connection and WebRTC signaling
let blazorComponent = null;
let signalRConnection = null;
let currentSessionId = null;

window.setSignalRFunctions = function(componentRef) {
    blazorComponent = componentRef;
    console.log('[TutoringSession] SignalR functions set up');
};

// Initialize SignalR connection
window.initializeSignalRConnection = async function(sessionId) {
    try {
        currentSessionId = sessionId;
        console.log('[TutoringSession] Initializing SignalR connection for session:', sessionId);
        
        // Use the globally loaded SignalR
        const signalR = window.signalR;
        
        // Create connection
        signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7228/tutoringsessionhub")
            .build();

        // Set up event handlers
        signalRConnection.on("UserJoined", (connectionId) => {
            console.log('[TutoringSession] User joined:', connectionId);
            // Ignore our own join event
            const selfId = signalRConnection.connectionId || signalRConnection.connection?.connectionId;
            if (connectionId === selfId) {
                return;
            }
            if (blazorComponent) {
                blazorComponent.invokeMethodAsync('HandleUserJoined', connectionId);
            }
        });

        signalRConnection.on("UserLeft", (connectionId) => {
            console.log('[TutoringSession] User left:', connectionId);
            if (blazorComponent) {
                blazorComponent.invokeMethodAsync('HandleUserLeft', connectionId);
            }
        });

        signalRConnection.on("ReceiveOffer", (fromConnectionId, offer) => {
            console.log('[TutoringSession] Received offer from:', fromConnectionId);
            window.handleReceiveOffer(fromConnectionId, offer);
        });

        signalRConnection.on("ReceiveAnswer", (fromConnectionId, answer) => {
            console.log('[TutoringSession] Received answer from:', fromConnectionId);
            window.handleReceiveAnswer(fromConnectionId, answer);
        });

        signalRConnection.on("ReceiveIceCandidate", (fromConnectionId, candidate) => {
            console.log('[TutoringSession] Received ICE candidate from:', fromConnectionId);
            window.handleReceiveIceCandidate(fromConnectionId, candidate);
        });

        // Start connection
        await signalRConnection.start();
        console.log('[TutoringSession] SignalR connection started');

        // Join session
        await signalRConnection.invoke("JoinSession", sessionId);
        console.log('[TutoringSession] Joined session:', sessionId);

    } catch (error) {
        console.error('[TutoringSession] Error initializing SignalR:', error);
    }
};

// Send WebRTC signaling messages via SignalR
window.sendOfferViaSignalR = async function(sessionId, targetConnectionId, offer) {
    if (signalRConnection) {
        await signalRConnection.invoke("SendOffer", sessionId, targetConnectionId, offer);
    }
};

window.sendAnswerViaSignalR = async function(sessionId, targetConnectionId, answer) {
    if (signalRConnection) {
        await signalRConnection.invoke("SendAnswer", sessionId, targetConnectionId, answer);
    }
};

window.sendIceCandidateViaSignalR = async function(sessionId, targetConnectionId, candidate) {
    if (signalRConnection) {
        await signalRConnection.invoke("SendIceCandidate", sessionId, targetConnectionId, candidate);
    }
};

// Handle user joined/left events
window.handleUserJoined = function(connectionId) {
    console.log('[TutoringSession] Handling user joined:', connectionId);
    if (!otherUserConnectionId) {
        otherUserConnectionId = connectionId;
        isInitiator = true;
        console.log('[TutoringSession] Setting as initiator, creating offer...');
        // Create offer when another user joins
        setTimeout(() => {
            createOffer();
        }, 1000);
    }
};

window.handleUserLeft = function(connectionId) {
    console.log('[TutoringSession] Handling user left:', connectionId);
    if (otherUserConnectionId === connectionId) {
        otherUserConnectionId = null;
        isInitiator = false;
        // Hide remote video
        const remoteVideo = document.getElementById('remoteVideo');
        if (remoteVideo) {
            remoteVideo.style.display = 'none';
            remoteVideo.srcObject = null;
        }
    }
};

window.sendOffer = function(targetConnectionId, offer) {
    console.log('[TutoringSession] Sending offer via SignalR');
    if (blazorComponent) {
        blazorComponent.invokeMethodAsync('SendOffer', targetConnectionId, offer);
    }
};

window.sendAnswer = function(targetConnectionId, answer) {
    console.log('[TutoringSession] Sending answer via SignalR');
    if (blazorComponent) {
        blazorComponent.invokeMethodAsync('SendAnswer', targetConnectionId, answer);
    }
};

window.sendIceCandidate = function(targetConnectionId, candidate) {
    console.log('[TutoringSession] Sending ICE candidate via SignalR');
    if (blazorComponent) {
        blazorComponent.invokeMethodAsync('SendIceCandidate', targetConnectionId, candidate);
    }
};

// Cleanup media streams and SignalR connection
window.cleanupMedia = async function() {
    try {
        console.log('[TutoringSession] Starting media cleanup...');
        
        // Stop all tracks in the local stream
        if (localStream) {
            console.log('[TutoringSession] Stopping local stream tracks...');
            localStream.getTracks().forEach(track => {
                console.log(`[TutoringSession] Stopping track: ${track.kind}`);
                track.stop();
            });
            localStream = null;
        }

        // Close peer connection
        if (peerConnection) {
            console.log('[TutoringSession] Closing peer connection...');
            peerConnection.close();
            peerConnection = null;
        }

        // Close SignalR connection
        if (signalRConnection) {
            console.log('[TutoringSession] Closing SignalR connection...');
            if (currentSessionId) {
                await signalRConnection.invoke("LeaveSession", currentSessionId);
            }
            await signalRConnection.stop();
            signalRConnection = null;
        }

        // Clear video elements
        const localVideo = document.getElementById('localVideo');
        if (localVideo) {
            localVideo.srcObject = null;
            console.log('[TutoringSession] Cleared local video source');
        }

        const remoteVideo = document.getElementById('remoteVideo');
        if (remoteVideo) {
            remoteVideo.srcObject = null;
            console.log('[TutoringSession] Cleared remote video source');
        }

        // Reset state variables
        otherUserConnectionId = null;
        isInitiator = false;
        isVideoEnabled = true;
        isAudioEnabled = true;
        isScreenSharing = false;
        currentSessionId = null;

        console.log('[TutoringSession] Media cleanup completed');
    } catch (error) {
        console.error('[TutoringSession] Error cleaning up media:', error);
    }
};

// Scroll to bottom of messages
window.scrollToBottom = function(element) {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

// Trigger file upload
window.clickFileUpload = function() {
    const fileUpload = document.getElementById('fileUpload');
    if (fileUpload) {
        fileUpload.click();
    }
};

// Close window (for complete session)
window.close = function() {
    window.close();
};

// File upload handling
window.handleFileUpload = function(files) {
    console.log('Files uploaded:', files);
    // Handle file upload logic here
};

// Real-time updates (would integrate with SignalR)
window.setupRealTimeUpdates = function(sessionId) {
    // Set up SignalR connection for real-time chat and updates
    console.log('Setting up real-time updates for session:', sessionId);
};

// Initialize media when DOM is ready
window.initializeMediaWhenReady = async function() {
    console.log('[TutoringSession] initializeMediaWhenReady called');
    
    // Wait longer for Blazor to render the DOM
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Check if we already have media initialized
    if (localStream) {
        console.log('[TutoringSession] Media already initialized');
        return;
    }
    
    console.log('[TutoringSession] Initializing media when DOM is ready...');
    await window.initializeMedia();
};

// File upload functions
let isFileUploadInProgress = false;

window.uploadFileToSession = async function(sessionId, uploaderId) {
    try {
        // Prevent multiple simultaneous uploads
        if (isFileUploadInProgress) {
            console.log('[TutoringSession] File upload already in progress, ignoring request');
            return 'busy';
        }
        
        isFileUploadInProgress = true;
        console.log('[TutoringSession] Starting file upload for session:', sessionId);
        
        // Create file input element if it doesn't exist
        let fileInput = document.getElementById('fileUpload');
        if (!fileInput) {
            fileInput = document.createElement('input');
            fileInput.type = 'file';
            fileInput.id = 'fileUpload';
            fileInput.style.display = 'none';
            fileInput.multiple = true;
            fileInput.accept = '.pdf,.doc,.docx,.ppt,.pptx,.jpg,.jpeg,.png,.gif,.txt';
            document.body.appendChild(fileInput);
        }
        
        // Create a promise to handle file selection
        return new Promise((resolve) => {
            // Clear any existing event listeners to prevent multiple handlers
            fileInput.onchange = null;
            
            fileInput.onchange = async function(event) {
                try {
                    const files = event.target.files;
                    if (files.length === 0) {
                        resolve('cancelled');
                        return;
                    }
                    
                    for (let i = 0; i < files.length; i++) {
                        const file = files[i];
                        console.log('[TutoringSession] Uploading file:', file.name);
                        
                        // Validate file size (10MB limit)
                        if (file.size > 10 * 1024 * 1024) {
                            alert(`File '${file.name}' is too large. Maximum size is 10MB.`);
                            continue;
                        }
                        
                        // Validate file type
                        const allowedTypes = ['pdf', 'doc', 'docx', 'ppt', 'pptx', 'jpg', 'jpeg', 'png', 'gif', 'txt'];
                        const fileExtension = file.name.split('.').pop().toLowerCase();
                        if (!allowedTypes.includes(fileExtension)) {
                            alert(`File type '${fileExtension}' is not allowed. Supported types: PDF, DOC, DOCX, PPT, PPTX, JPG, PNG, GIF, TXT`);
                            continue;
                        }
                        
                               // Create FormData
                               const formData = new FormData();
                               formData.append('File', file);
                               formData.append('UploaderId', uploaderId);
                        
                        // Upload file
                        const response = await fetch(`https://localhost:7228/api/sessions/${sessionId}/resources`, {
                            method: 'POST',
                            body: formData
                        });
                        
                        if (response.ok) {
                            const result = await response.json();
                            console.log('[TutoringSession] File uploaded successfully:', result);
                        } else {
                            const errorText = await response.text();
                            console.error('[TutoringSession] Upload failed:', response.status, errorText);
                            alert(`Failed to upload file '${file.name}': ${response.status}`);
                        }
                    }
                    
                    resolve('success');
                } catch (error) {
                    console.error('[TutoringSession] Error uploading files:', error);
                    alert('Error uploading files: ' + error.message);
                    resolve('error');
                } finally {
                    // Reset file input and clear the flag
                    fileInput.value = '';
                    isFileUploadInProgress = false;
                }
            };
            
            // Add a timeout to prevent hanging
            setTimeout(() => {
                if (isFileUploadInProgress) {
                    console.log('[TutoringSession] File upload timeout, resetting state');
                    isFileUploadInProgress = false;
                    resolve('timeout');
                }
            }, 30000); // 30 second timeout
            
            // Trigger file selection dialog
            try {
                fileInput.click();
            } catch (error) {
                console.error('[TutoringSession] Error opening file dialog:', error);
                isFileUploadInProgress = false;
                resolve('error');
            }
        });
        
    } catch (error) {
        console.error('[TutoringSession] Error in uploadFileToSession:', error);
        isFileUploadInProgress = false;
        return 'error';
    }
};

// Enhanced file upload with drag and drop
window.setupFileUpload = function() {
    const uploadArea = document.querySelector('.upload-area');
    if (!uploadArea) return;
    
    // Prevent default drag behaviors
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        uploadArea.addEventListener(eventName, preventDefaults, false);
        document.body.addEventListener(eventName, preventDefaults, false);
    });
    
    // Highlight drop area when item is dragged over it
    ['dragenter', 'dragover'].forEach(eventName => {
        uploadArea.addEventListener(eventName, highlight, false);
    });
    
    ['dragleave', 'drop'].forEach(eventName => {
        uploadArea.addEventListener(eventName, unhighlight, false);
    });
    
    // Handle dropped files
    uploadArea.addEventListener('drop', handleDrop, false);
    
    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }
    
    function highlight(e) {
        uploadArea.classList.add('drag-over');
    }
    
    function unhighlight(e) {
        uploadArea.classList.remove('drag-over');
    }
    
    function handleDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;
        
        console.log('[TutoringSession] Files dropped:', files.length);
        
        if (files.length > 0) {
            // Trigger file upload for each file
            Array.from(files).forEach(file => {
                // Simulate file input change event
                const event = new Event('change');
                const fileInput = document.getElementById('fileUpload');
                if (fileInput) {
                    // Create a new FileList-like object
                    const dataTransfer = new DataTransfer();
                    dataTransfer.items.add(file);
                    fileInput.files = dataTransfer.files;
                    fileInput.dispatchEvent(event);
                }
            });
        }
    }
};

// Initialize file upload when DOM is ready
window.initializeFileUpload = function() {
    console.log('[TutoringSession] Initializing file upload functionality...');
    window.setupFileUpload();
};

// Debug function to check WebRTC status
window.debugWebRTCStatus = function() {
    console.log('[TutoringSession] === WebRTC Debug Status ===');
    console.log('Local Stream:', localStream ? 'Available' : 'Not available');
    console.log('Peer Connection:', peerConnection ? 'Available' : 'Not available');
    console.log('Other User Connection ID:', otherUserConnectionId || 'Not set');
    console.log('Is Initiator:', isInitiator);
    console.log('SignalR Connection:', signalRConnection ? 'Connected' : 'Not connected');
    console.log('Current Session ID:', currentSessionId || 'Not set');
    
    if (peerConnection) {
        console.log('Connection State:', peerConnection.connectionState);
        console.log('ICE Connection State:', peerConnection.iceConnectionState);
        console.log('ICE Gathering State:', peerConnection.iceGatheringState);
    }
    
    if (localStream) {
        const videoTracks = localStream.getVideoTracks();
        const audioTracks = localStream.getAudioTracks();
        console.log('Video Tracks:', videoTracks.length);
        console.log('Audio Tracks:', audioTracks.length);
        
        videoTracks.forEach((track, index) => {
            console.log(`Video Track ${index}:`, {
                enabled: track.enabled,
                readyState: track.readyState,
                label: track.label
            });
        });
        
        audioTracks.forEach((track, index) => {
            console.log(`Audio Track ${index}:`, {
                enabled: track.enabled,
                readyState: track.readyState,
                label: track.label
            });
        });
    }
    
    console.log('[TutoringSession] === End Debug Status ===');
};

// Make debug function available globally
window.debugTutoringSession = window.debugWebRTCStatus;