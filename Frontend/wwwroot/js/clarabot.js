window.initializeBotpress = function (dotnetRef) {
    window.claraDotNetRef = dotnetRef;
    const container = document.getElementById('bp-container');

    const iframe = document.createElement('iframe');
    iframe.src = 'https://cdn.botpress.cloud/webchat/v3.3/shareable.html?configUrl=https://files.bpcontent.cloud/2025/10/16/18/20251016180452-SK0Z5GSL.json';
    iframe.style.width = '100%';
    iframe.style.height = '600px';
    iframe.style.border = 'none';
    iframe.id = 'bp-iframe';
    container.appendChild(iframe);

    window.addEventListener('message', function (event) {
        const data = event.data;
        if (!data) return;

        if (data.type === 'webchat.ready') {
            window.claraDotNetRef.invokeMethodAsync('OnBotpressReady');
        }

        if (data.type === 'webchat.message') {
            if (data.payload && data.payload.text) {
                window.claraDotNetRef.invokeMethodAsync('ReceiveBotMessage', data.payload.text);
            }
        }
    });

    window.sendToBotpress = function (text) {
        const iframe = document.getElementById('bp-iframe');
        if (!iframe) return;

        // Send message using proactive-trigger (Botpress expects this)
        iframe.contentWindow.postMessage({
            type: 'proactive-trigger',
            payload: { type: 'text', text: text }
        }, '*');
    };

    window.resetBotpressConversation = function () {
        const iframe = document.getElementById('bp-iframe');
        if (!iframe) return;
        iframe.contentWindow.postMessage({ type: 'webchat.reset' }, '*');
    };
};
