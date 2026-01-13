window.cryptoFunctions = {
    currentVaultKey: null, // Store the decrypted key here

    createVaultCrypto: async function (password) {
        try {
            // 1. Generate Salt (64 bytes)
            const salt = window.crypto.getRandomValues(new Uint8Array(64));

            // 2. Generate Vault Key (32 bytes)
            const vaultKey = window.crypto.getRandomValues(new Uint8Array(32));

            // 3. Derive KEK from password + salt
            const passwordBuffer = new TextEncoder().encode(password);
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
                    salt: salt,
                    iterations: 100000,
                    hash: "SHA-256",
                },
                baseKey,
                256 // 256 bits = 32 bytes
            );

            const kek = await window.crypto.subtle.importKey(
                "raw",
                derivedBits,
                { name: "AES-GCM" },
                false,
                ["encrypt"]
            );

            // 4. Encrypt Vault Key with KEK
            const iv = window.crypto.getRandomValues(new Uint8Array(12));
            const encryptedVaultKey = await window.crypto.subtle.encrypt(
                {
                    name: "AES-GCM",
                    iv: iv,
                },
                kek,
                vaultKey
            );

            // Concatenate IV + EncryptedData
            const resultBuffer = new Uint8Array(iv.byteLength + encryptedVaultKey.byteLength);
            resultBuffer.set(iv, 0);
            resultBuffer.set(new Uint8Array(encryptedVaultKey), iv.byteLength);

            return {
                masterSalt: this.arrayBufferToBase64(salt.buffer),
                encryptedKey: this.arrayBufferToBase64(resultBuffer.buffer)
            };

        } catch (e) {
            console.error("Erreur JS dans createVaultCrypto:", e);
            throw e;
        }
    },

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

            // Store the key in memory (ArrayBuffer)
            this.currentVaultKey = decrypted;
            
            return true; // Success

        } catch (e) {
            console.error("Erreur de déchiffrement (deriveKeyAndDecrypt):", e.message);
            this.currentVaultKey = null;
            throw e;
        }
    },

    decryptData: async function (encryptedData) {
        if (!this.currentVaultKey) {
            throw new Error("Vault key not available. Please unlock the vault first.");
        }

        try {
            const encryptedDataBuffer = this.base64ToArrayBuffer(encryptedData);
            const iv = encryptedDataBuffer.slice(0, 12);
            const data = encryptedDataBuffer.slice(12);

            const cryptoKey = await window.crypto.subtle.importKey(
                "raw",
                this.currentVaultKey,
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

    encryptData: async function (plainTextData) {
        if (!this.currentVaultKey) {
            throw new Error("Vault key not available. Please unlock the vault first.");
        }

        try {
            const dataBuffer = new TextEncoder().encode(plainTextData);

            const cryptoKey = await window.crypto.subtle.importKey(
                "raw",
                this.currentVaultKey,
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
    
    // --- NOUVELLES FONCTIONS POUR ZERO-KNOWLEDGE STRICT ---

    // Lit la valeur d'un input HTML, la chiffre et retourne le résultat chiffré
    encryptInputValue: async function (elementId) {
        const element = document.getElementById(elementId);
        if (!element) return "";
        
        const value = element.value;
        if (!value) return ""; 
        
        return await this.encryptData(value);
    },

    // Lit plusieurs inputs, construit un JSON, le chiffre et retourne le résultat chiffré
    encryptEntryData: async function (titleId, usernameId) {
        const titleEl = document.getElementById(titleId);
        const usernameEl = document.getElementById(usernameId);
        
        const data = {
            Title: titleEl ? titleEl.value : "",
            Username: usernameEl ? usernameEl.value : ""
        };
        
        const jsonData = JSON.stringify(data);
        return await this.encryptData(jsonData);
    },

    // Déchiffre une donnée et remplit directement un input HTML
    decryptAndFillInput: async function (encryptedData, elementId) {
        if (!encryptedData) return;
        
        try {
            const decryptedValue = await this.decryptData(encryptedData);
            const element = document.getElementById(elementId);
            if (element) {
                element.value = decryptedValue;
            }
        } catch (e) {
            console.error("Erreur lors du remplissage de l'input:", e);
        }
    },
    
    // Déchiffre une entrée complète et remplit les champs d'affichage (Titre, Username)
    decryptAndFillEntry: async function (encryptedData, titleId, usernameId) {
        if (!encryptedData) return;

        try {
            const jsonString = await this.decryptData(encryptedData);
            const data = JSON.parse(jsonString);

            const titleEl = document.getElementById(titleId);
            if (titleEl) titleEl.innerText = data.Title || "";

            const usernameEl = document.getElementById(usernameId);
            if (usernameEl) usernameEl.innerText = data.Username || "";

        } catch (e) {
            console.error("Erreur lors du déchiffrement de l'entrée:", e);
        }
    },
    
    // Déchiffre et affiche le mot de passe dans un élément spécifique
    decryptAndShowPassword: async function (encryptedPassword, elementId) {
        if (!encryptedPassword) return;
        
        try {
            const decryptedPassword = await this.decryptData(encryptedPassword);
            const element = document.getElementById(elementId);
            if (element) {
                element.innerText = decryptedPassword;
            }
        } catch (e) {
            console.error("Erreur lors du déchiffrement du mot de passe:", e);
        }
    },

    clearKey: function() {
        this.currentVaultKey = null;
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
