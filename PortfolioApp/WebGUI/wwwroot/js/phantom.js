window.phantomInterop = {
    connectWallet: async function () {
        if (!window.solana || !window.solana.isPhantom) {
            alert('Phantom wallet not found');
            throw 'Phantom wallet not found';
        }
        const res = await window.solana.connect();
        return res.publicKey.toString();
    },

    signAndSend: async function (txBase64) {
        if (!window.solana || !window.solana.isPhantom) {
            alert('Phantom wallet not found');
            throw 'Phantom wallet not found';
        }

        const provider = window.solana;

        // WICHTIG: VersionedTransaction kommt aus globalem solanaWeb3
        const { VersionedTransaction } = solanaWeb3;

        const txBytes = Uint8Array.from(atob(txBase64), c => c.charCodeAt(0));
        const transaction = VersionedTransaction.deserialize(txBytes);

        const { signature } = await provider.signAndSendTransaction(transaction);
        return signature;
    }
};