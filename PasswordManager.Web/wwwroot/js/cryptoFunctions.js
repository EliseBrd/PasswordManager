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

    changeMasterPassword: async function (newPassword) {
        if (!this.currentVaultKey) {
            throw new Error("Vault is locked. Cannot change password.");
        }

        try {
            // 1. Generate New Salt (64 bytes)
            const salt = window.crypto.getRandomValues(new Uint8Array(64));

            // 2. Derive New KEK from newPassword + salt
            const passwordBuffer = new TextEncoder().encode(newPassword);
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

            // 3. Encrypt EXISTING Vault Key with New KEK
            const iv = window.crypto.getRandomValues(new Uint8Array(12));
            const encryptedVaultKey = await window.crypto.subtle.encrypt(
                {
                    name: "AES-GCM",
                    iv: iv,
                },
                kek,
                this.currentVaultKey // Encrypt the existing decrypted vault key
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
            console.error("Erreur JS dans changeMasterPassword:", e);
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

    // --- FONCTIONS POUR LES LOGS ---

    // Crée un log chiffré
    encryptLog: async function (actionType, description, userEmail) {
        const logData = {
            Type: actionType,
            Text: description,
            User: userEmail
        };
        
        const jsonData = JSON.stringify(logData);
        return await this.encryptData(jsonData);
    },
    
    // Crée un log de CRÉATION avec le titre de l'entrée
    encryptCreateLog: async function (titleInputId, userEmail) {
        const title = document.getElementById(titleInputId)?.value || "Nouvelle entrée";
        const description = `Création de l'entrée '${title}'`;
        return await this.encryptLog("CreateEntry", description, userEmail);
    },
    
    // Crée un log de SUPPRESSION avec le titre de l'entrée (récupéré du DOM)
    encryptDeleteLog: async function (entryId, userEmail) {
        const titleEl = document.getElementById(`title-${entryId}`);
        const title = titleEl ? titleEl.innerText : "Entrée inconnue";
        const description = `Suppression de l'entrée '${title}'`;
        return await this.encryptLog("DeleteEntry", description, userEmail);
    },
    
    // Crée un log de CONSULTATION DE MOT DE PASSE avec le titre de l'entrée
    encryptShowPasswordLog: async function (entryId, userEmail) {
        const titleEl = document.getElementById(`title-${entryId}`);
        const title = titleEl ? titleEl.innerText : "Entrée inconnue";
        const description = `Consultation du mot de passe de '${title}'`;
        return await this.encryptLog("ShowPassword", description, userEmail);
    },
    
    // Crée un log de mise à jour en comparant les anciennes et nouvelles valeurs
    encryptUpdateLog: async function (oldEncryptedData, titleInputId, usernameInputId, passwordInputId, userEmail) {
        let description = "Modification d'une entrée";
        
        try {
            // 1. Déchiffrer l'ancienne valeur
            const oldJsonString = await this.decryptData(oldEncryptedData);
            const oldData = JSON.parse(oldJsonString);
            const entryTitle = oldData.Title || "Entrée";
            
            // 2. Lire les nouvelles valeurs
            const newTitle = document.getElementById(titleInputId)?.value || "";
            const newUsername = document.getElementById(usernameInputId)?.value || "";
            const newPassword = document.getElementById(passwordInputId)?.value || "";
            
            // 3. Comparer
            const changes = [];
            
            if (oldData.Title !== newTitle) {
                changes.push(`Titre modifié ('${oldData.Title}' -> '${newTitle}')`);
            }
            
            if (oldData.Username !== newUsername) {
                changes.push(`Nom d'utilisateur modifié ('${oldData.Username}' -> '${newUsername}')`);
            }
            
            if (newPassword && newPassword.length > 0) {
                changes.push("Mot de passe modifié");
            }
            
            if (changes.length > 0) {
                description = `Modification de '${entryTitle}' : ${changes.join(", ")}`;
            } else {
                description = `Modification de '${entryTitle}' (aucune donnée changée)`;
            }
            
        } catch (e) {
            console.error("Erreur lors de la comparaison pour le log:", e);
            description = "Modification d'une entrée (détails indisponibles)";
        }
        
        // 4. Chiffrer le log
        return await this.encryptLog("UpdateEntry", description, userEmail);
    },

    // Déchiffre une liste de logs et génère le HTML directement dans le conteneur
    // Zero-Knowledge : Le serveur ne reçoit jamais les logs en clair
    renderDecryptedLogs: async function (encryptedLogs, containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;
        
        container.innerHTML = ""; // Vider le conteneur
        
        if (!encryptedLogs || encryptedLogs.length === 0) {
            container.innerHTML = `
                <div class="text-center py-4 text-muted">
                    <i class="fa-regular fa-folder-open fa-2x mb-2"></i>
                    <p>Aucun historique disponible.</p>
                </div>`;
            return;
        }

        // Création de la table
        const table = document.createElement("table");
        table.className = "table table-hover align-middle";
        
        const thead = document.createElement("thead");
        thead.className = "table-light";
        thead.innerHTML = `
            <tr>
                <th scope="col" style="width: 180px;">Date</th>
                <th scope="col" style="width: 120px;">Action</th>
                <th scope="col">Détails</th>
                <th scope="col">Utilisateur</th>
            </tr>`;
        table.appendChild(thead);
        
        const tbody = document.createElement("tbody");
        
        for (const log of encryptedLogs) {
            const tr = document.createElement("tr");
            
            // Date (en clair)
            const dateCell = document.createElement("td");
            dateCell.className = "text-muted small";
            dateCell.innerText = new Date(log.date).toLocaleString();
            tr.appendChild(dateCell);
            
            // Déchiffrement
            let type = "Unknown";
            let text = "Erreur de déchiffrement";
            let user = "Unknown";
            
            try {
                const jsonString = await this.decryptData(log.encryptedData);
                const data = JSON.parse(jsonString);
                type = data.Type || "Unknown";
                text = data.Text || "";
                user = data.User || "";
            } catch (e) {
                console.error("Erreur déchiffrement log:", e);
            }
            
            // Action (Badge)
            const actionCell = document.createElement("td");
            const badge = document.createElement("span");
            badge.className = "badge " + this.getBadgeClass(type);
            badge.innerText = this.getActionLabel(type);
            actionCell.appendChild(badge);
            tr.appendChild(actionCell);
            
            // Détails
            const textCell = document.createElement("td");
            textCell.innerText = text;
            tr.appendChild(textCell);
            
            // Utilisateur
            const userCell = document.createElement("td");
            userCell.className = "small text-muted";
            userCell.innerHTML = `<i class="fa-solid fa-user me-1"></i> ${user}`;
            tr.appendChild(userCell);
            
            tbody.appendChild(tr);
        }
        
        table.appendChild(tbody);
        
        // Wrapper responsive
        const wrapper = document.createElement("div");
        wrapper.className = "table-responsive";
        wrapper.appendChild(table);
        
        container.appendChild(wrapper);
    },
    
    getBadgeClass: function(type) {
        switch(type) {
            case "CreateEntry": return "bg-success";
            case "UpdateEntry": return "bg-warning text-dark";
            case "DeleteEntry": return "bg-danger";
            case "ShowPassword": return "bg-primary"; // Nouveau type
            case "UpdateVault": return "bg-info text-dark";
            default: return "bg-secondary";
        }
    },
    
    getActionLabel: function(type) {
        switch(type) {
            case "CreateEntry": return "Création";
            case "UpdateEntry": return "Modification";
            case "DeleteEntry": return "Suppression";
            case "ShowPassword": return "Consultation"; // Nouveau type
            case "UpdateVault": return "Config Coffre";
            default: return type;
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
    },

    // Déchiffre une entrée et remplit les inputs de la MODAL (édition)
    decryptAndFillEditModal: async function (encryptedData, titleInputId, usernameInputId) {
        if (!encryptedData) return;

        try {
            const jsonString = await this.decryptData(encryptedData);
            const data = JSON.parse(jsonString);

            const titleInput = document.getElementById(titleInputId);
            if (titleInput) titleInput.value = data.Title || "";

            const usernameInput = document.getElementById(usernameInputId);
            if (usernameInput) usernameInput.value = data.Username || "";

        } catch (e) {
            console.error("Erreur lors du remplissage de la modal d'édition:", e);
        }
    },

    hidePassword: function (entryId) {
        const el = document.getElementById(`password-${entryId}`);
        if (el) {
            el.innerText = "••••••••";
        }
    }

};
