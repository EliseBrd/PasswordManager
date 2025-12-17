window.cryptoFunctions = {
    deriveKeyAndDecrypt: async function (password, salt, encryptedKey) {
        try {
            const passwordBuffer = new TextEncoder().encode(password);
            const saltBuffer = this.base64ToArrayBuffer(salt);

            const baseKey = await window.crypto.subtle.importKey(
                "raw",
                passwordBuffer,
                { name: "PBKDF2" },
                false,
                ["deriveBits"]
            );

            const derivedBits = await window.crypto.subtle.deriveBits(
                {
                    name: "PBKDF2",
                    salt: saltBuffer,
                    iterations: 100000,
                    hash: "SHA-256",
                },
                baseKey,
                256 // 256 bits = 32 bytes
            );

            const aesKey = await window.crypto.subtle.importKey(
                "raw",
                derivedBits,
                { name: "AES-GCM" },
                false,
                ["decrypt"]
            );

            const encryptedKeyBuffer = this.base64ToArrayBuffer(encryptedKey);
            const iv = encryptedKeyBuffer.slice(0, 12);
            const data = encryptedKeyBuffer.slice(12);

            const decrypted = await window.crypto.subtle.decrypt(
                {
                    name: "AES-GCM",
                    iv: iv,
                },
                aesKey,
                data
            );

            return this.arrayBufferToBase64(decrypted);

        } catch (e) {
            console.error("Erreur de déchiffrement (deriveKeyAndDecrypt):", e.message);
            throw e;
        }
    },

    decryptData: async function (decryptedKeyBase64, encryptedData) {
        try {
            const keyBuffer = this.base64ToArrayBuffer(decryptedKeyBase64);
            const encryptedDataBuffer = this.base64ToArrayBuffer(encryptedData);
            const iv = encryptedDataBuffer.slice(0, 12);
            const data = encryptedDataBuffer.slice(12);

            const cryptoKey = await window.crypto.subtle.importKey(
                "raw",
                keyBuffer,
                { name: "AES-GCM" },
                false,
                ["decrypt"]
            );

            const decrypted = await window.crypto.subtle.decrypt(
                {
                    name: "AES-GCM",
                    iv: iv,
                },
                cryptoKey,
                data
            );

            return new TextDecoder().decode(decrypted);
        } catch (e) {
            console.error("Erreur de déchiffrement (decryptData):", e.message);
            throw e;
        }
    },

    encryptData: async function (decryptedKeyBase64, plainTextData) {
        try {
            const keyBuffer = this.base64ToArrayBuffer(decryptedKeyBase64);
            const dataBuffer = new TextEncoder().encode(plainTextData);

            const cryptoKey = await window.crypto.subtle.importKey(
                "raw",
                keyBuffer,
                { name: "AES-GCM" },
                false,
                ["encrypt"]
            );

            const iv = window.crypto.getRandomValues(new Uint8Array(12));

            const encrypted = await window.crypto.subtle.encrypt(
                {
                    name: "AES-GCM",
                    iv: iv,
                },
                cryptoKey,
                dataBuffer
            );

            const resultBuffer = new Uint8Array(iv.byteLength + encrypted.byteLength);
            resultBuffer.set(iv, 0);
            resultBuffer.set(new Uint8Array(encrypted), iv.byteLength);

            return this.arrayBufferToBase64(resultBuffer.buffer);

        } catch (e) {
            console.error("Erreur de chiffrement (encryptData):", e.message);
            throw e;
        }
    },

    base64ToArrayBuffer: function (base64) {
        const binaryString = window.atob(base64);
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes.buffer;
    },

    arrayBufferToBase64: function (buffer) {
        let binary = '';
        const bytes = new Uint8Array(buffer);
        const len = bytes.byteLength;
        for (let i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    }
};
