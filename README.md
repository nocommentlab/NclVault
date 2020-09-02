# NclVault

![banner](https://raw.githubusercontent.com/nocommentlab/NclVault/master/readme_media/banner.jpg)

NclVault is an open source project written by the **!//Lab** completely in .Net Core. It runs on Windows, Mac and Linux.

**The NclVault is under development, you could lost your data, use it and pay attention!** 

## tl;dr

We use every day more credential to access to infinite online services. Someone uses online password manager like Lastpass, someone uses offline password manager but every type of Password Manager has its weak point.

- **Online Password Manager**: there are a lot of free services that permit to store our passwords on its servers. Free... Does the online Password Manager sell my information to third part? It's secure? What happens if someone hacks the service provider server?
- **Offline Password Manger**: like Keepass, every time I need to see a credential I must turn on my PC... Ok, we can copy the database on all of our devices, but when we need to update a credential? We need to deploy the new database manually.

NclVault is completely written in ASP.Net core API and allows you to store your credentials in secure mode.

## How it works

This repository contains two projects:

- NclVaultAPIServer: this is the core. Written in ASP.Net Core API, it exposes the REST API that the clients connect to.
- NclVaultCLIClient: a simple .net core console application used to demonstrate how the API core server works.

## NclVaultAPIServer

### Init Vault Workflow

The below workflow shows how NCLVault initializes the database that will be used to store the credentials.

[![init_vault_worflow](https://raw.githubusercontent.com/nocommentlab/NclVault/master/readme_media/NCLVault-InitWorkflows.png)](https://www.youtube.com/watch?v=kyR2TsowTUg)

1. The client sends to the server a POST HTTP request to `vault/initvault/` API Endopoint with the below body:

```
{
    "username": "antonio",
    "password": "myPassword2020"
}
```

2. NCLVault Server reponses to the client with a JSON object that contains the `InitId` property. This property is random generated every each init command. ```STORE THE INITID VALUE SOMEWHERE!! WITHOUT IT YOU CAN'T RECOVER YOUR STORED PASSWORD```

3. The Client stores the `InitId` inside a sicure element. The NCLVaultCLIClient uses the DataProtected protection mechanism to store it.

4. The NCLVault Server encrypts the database with AES256 CBC and stores the user password with `SHA256(password+salt)` hash.

### Login Vault Workflow

The below workflow shows how NCLVault permits the login to user that has initilizes the database.

[![login_vault_worflow](https://raw.githubusercontent.com/nocommentlab/NclVault/master/readme_media/NCLVault-LoginWorkflows.png)](https://www.youtube.com/watch?v=duUElcJHyWo)

1. The client sends to the server a POST HTTP request to `token/login` API Endopoint with the below body:

```
{
    "username": "antonio",
    "password": "myPassword2020"
}
```

2. NCLVault Server decrypts the database and checks the credential sent from the client

3. NCLVault server sents, if the credential is correct, the JWT token inside the X-Token HTTP Header like this:

```
{
    alg: "HS256",
    typ: "JWT"
}.
{
    username: "antonio",
    exp: 1599042782,
    iss: "NCLVault"
}.
[signature]
```

The claim `username` identifies the user that has correct logged on.

### Create Password Vault Workflow

The below workflow shows how NCLVault permits the creation of a new entry inside its database.

[![create_password_vault_workflow](https://raw.githubusercontent.com/nocommentlab/NclVault/master/readme_media/NCLVault-CreatePasswordWorkflow.png)](https://www.youtube.com/watch?v=HHvWcTR-ufg)

1. The client decrypts the protected `InitId` key stored locally

2. The client sends to the server a POST HTTP request to `vault/create/password` API Endopoint with the below body:

    ```
    {
        "name":"First credential",
        "username": "a.blescia@nocommentlab",
        "password": "Pass123!!"
    }
    ```

    In this request, the client adds the HTTP Header `InitId` previously extracted.

3. NCLVault Server decrypts the database and encrypt the received password il clear-text using the `Initid` received by the client. The password encryption is made with `AES 256` with `CBC mode` and random `IV`.

4. NCLVault Server response to the client with the created entry. The password in not in clear-text in the response.

```
{
    "username": "a.blescia@nocommentlab",
    "password": "0yec86Fkt7iQ8BuFJOfFQLuHbNyj7RcLRC6N79hbkEk=",
    "url": null,
    "id": 1,
    "group": null,
    "name": "First credential",
    "expired": "0001-01-01T00:00:00",
    "notes": null
}
```

### Read Password Vault Workflow

The below workflow shows how NCLVault permits the extraction of an entry from its database.

[![read_password_vault_workflow](https://raw.githubusercontent.com/nocommentlab/NclVault/master/readme_media/NCLVault-ReadPasswordWorkflow.png)](https://www.youtube.com/watch?v=dMKHszX9u2c)

1. The client decrypts the protected `InitId` key stored locally

2. The client sends to the server a GET HTTP request to `vault/read/password/{id}` API Endopoint. The `{id}` identify the password entry

    In this request, the client adds the HTTP Header `InitId` previously extracted.

3. NCLVault Server decrypts the database and decrypt the relative password entry ID with the `Initid` received by the client. The password decryption is made with `AES 256` with `CBC mode`.

## To be implemented

- [ ] Code Quality
- [ ] Random Password Generator Module
- [ ] Permit Multiple Users
- [ ] Write Recovery Strategy
