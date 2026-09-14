# Overview
This is a REST API that implements CRUD operations on <code>Author</code> and <code>Book</code> entities. It automatically applies database migrations when it starts. In <code>BooksAPI.postman_collection.json</code> you can find the corresponding postman collection.

## Stack
- .NET 10
- MS SQL Server

## Caching
The <code>GET /authors</code> call caches it's response for 15 seconds, as long as the corresponding <code>POST</code>, <code>PUT</code> and <code>DELETE</code> endpoints are not being called in the meanwhile. This mechanism uses the Output Caching Middleware of .Net by adding the <code>Age</code> header to the response.

## Authorization
All <code>POST</code>, <code>PUT</code> and <code>DELETE</code> endpoints require Bearer Authentication. You can visit https://www.jwt.io/ to generate a valid JWT token. For the purposes of this demo, follow the below template:

#### Header
```
{
  "alg": "HS256",
  "typ": "JWT"
}
```

#### Payload
```
{
  "iss": "https://localhost:44316",
  "aud": "https://localhost:44316"
}
```

#### JWT Signature Verification
```
a-string-secret-at-least-256-bits-long
```