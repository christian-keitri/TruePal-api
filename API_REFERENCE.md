# TruePal.Api - API Reference

Base URL: `https://localhost:5001/api`

## Authentication

All protected endpoints require a JWT token in the `Authorization` header:
```
Authorization: Bearer <token>
```

Tokens are obtained via the login endpoint and expire after 60 minutes.

---

## Endpoints

### Auth

#### POST `/api/auth/register`

Register a new user account.

**Request:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| username | string | Yes | 3-50 characters |
| email | string | Yes | Valid email format |
| password | string | Yes | Minimum 6 characters |

**Success Response (200):**
```json
{
  "id": 1,
  "username": "johndoe",
  "email": "john@example.com"
}
```

**Error Responses:**

400 - Validation errors:
```json
{
  "errors": [
    "Username must be between 3 and 50 characters",
    "Password must be at least 6 characters"
  ]
}
```

400 - Duplicate:
```json
{
  "error": "Email already exists"
}
```

---

#### POST `/api/auth/login`

Authenticate and receive a JWT token.

**Request:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass123!"
}
```

| Field | Type | Required |
|-------|------|----------|
| email | string | Yes |
| password | string | Yes |

**Success Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Error Responses:**

401 - Invalid credentials:
```json
{
  "error": "Invalid email or password"
}
```

401 - Validation errors:
```json
{
  "errors": ["Email is required", "Password is required"]
}
```

---

### Posts

#### GET `/api/posts/{id}`

Get a single post by ID.

**Parameters:**
| Parameter | Location | Type | Required |
|-----------|----------|------|----------|
| id | path | int | Yes |

**Success Response (200):**
```json
{
  "id": 1,
  "content": "Hello TruePal!",
  "location": "Manila",
  "imageUrl": "https://example.com/photo.jpg",
  "likesCount": 12,
  "commentsCount": 3,
  "viewsCount": 45,
  "trendingScore": null,
  "createdAt": "2026-04-15T08:30:00Z",
  "updatedAt": null,
  "user": {
    "id": 1,
    "username": "johndoe"
  }
}
```

**Error Response (404):**
```json
{
  "error": "Post not found"
}
```

---

#### GET `/api/posts/recent`

Get recent posts ordered by creation date (newest first).

**Query Parameters:**
| Parameter | Type | Default | Range |
|-----------|------|---------|-------|
| count | int | 10 | 1-100 |

**Success Response (200):**
```json
[
  {
    "id": 3,
    "content": "Latest post",
    "location": null,
    "imageUrl": null,
    "likesCount": 0,
    "commentsCount": 0,
    "viewsCount": 0,
    "trendingScore": null,
    "createdAt": "2026-04-15T10:00:00Z",
    "updatedAt": null,
    "user": {
      "id": 1,
      "username": "johndoe"
    }
  }
]
```

**Error Response (400):**
```json
{
  "error": "Count must be between 1 and 100"
}
```

---

#### GET `/api/posts/trending`

Get posts ordered by engagement score (highest first).

**Trending Score Formula:** `(likes x 2) + (comments x 3) + (views x 0.5)`

**Query Parameters:**
| Parameter | Type | Default | Range |
|-----------|------|---------|-------|
| count | int | 10 | 1-100 |

**Success Response (200):**
```json
[
  {
    "id": 2,
    "content": "Popular post",
    "location": "Cebu",
    "imageUrl": null,
    "likesCount": 50,
    "commentsCount": 20,
    "viewsCount": 200,
    "trendingScore": 260.0,
    "createdAt": "2026-04-14T12:00:00Z",
    "updatedAt": null,
    "user": {
      "id": 1,
      "username": "johndoe"
    }
  }
]
```

Note: `trendingScore` is only included in the trending endpoint response.

---

#### GET `/api/posts/user/{userId}`

Get all posts by a specific user, ordered by creation date (newest first).

**Parameters:**
| Parameter | Location | Type | Required |
|-----------|----------|------|----------|
| userId | path | int | Yes |

**Success Response (200):** Array of post objects (same schema as recent, without `trendingScore`).

Returns empty array `[]` if the user has no posts.

---

#### POST `/api/posts`

Create a new post. **Requires authentication.**

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "content": "Hello from Manila!",
  "location": "Manila",
  "imageUrl": "https://example.com/photo.jpg"
}
```

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| content | string | Yes | 1-500 characters |
| location | string | No | Max 200 characters |
| imageUrl | string | No | Max 500 characters |

**Success Response (201 Created):**

Returns the created post (same schema as GET). Includes `Location` header pointing to the new resource.

**Error Responses:**

400 - Validation:
```json
{
  "errors": [
    "Post content is required",
    "Location must be 200 characters or less"
  ]
}
```

401 - Not authenticated:
```json
{
  "error": "Invalid authentication token"
}
```

---

#### PUT `/api/posts/{id}`

Update an existing post. **Requires authentication. Owner only.**

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "content": "Updated content!",
  "location": "Cebu",
  "imageUrl": null
}
```

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| content | string | Yes | 1-500 characters |
| location | string | No | Max 200 characters |
| imageUrl | string | No | Max 500 characters |

**Success Response (200):** Returns the updated post.

**Error Responses:**

400 - Validation: `{ "errors": [...] }`
401 - Not authenticated: `{ "error": "Invalid authentication token" }`
403 - Not owner: `{ "error": "You are not authorized to update this post" }`
404 - Not found: `{ "error": "Post not found" }`

---

#### DELETE `/api/posts/{id}`

Delete a post. **Requires authentication. Owner only.**

**Headers:**
```
Authorization: Bearer <token>
```

**Success Response (204 No Content):** Empty body.

**Error Responses:**

401 - Not authenticated: `{ "error": "Invalid authentication token" }`
403 - Not owner: `{ "error": "You are not authorized to delete this post" }`
404 - Not found: `{ "error": "Post not found" }`

---

#### POST `/api/posts/{id}/views`

Increment the view count for a post. No authentication required.

**Parameters:**
| Parameter | Location | Type | Required |
|-----------|----------|------|----------|
| id | path | int | Yes |

**Success Response (204 No Content):** Empty body.

**Error Response (404):**
```json
{
  "error": "Post not found"
}
```

---

## Error Format

All errors follow a consistent JSON format:

**Single error:**
```json
{
  "error": "Human-readable error message"
}
```

**Multiple validation errors:**
```json
{
  "errors": [
    "Content is required",
    "Location must be 200 characters or less"
  ]
}
```

## HTTP Status Code Summary

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET, PUT |
| 201 | Created | Successful POST (resource created) |
| 204 | No Content | Successful DELETE, view increment |
| 400 | Bad Request | Validation errors |
| 401 | Unauthorized | Missing/invalid JWT token |
| 403 | Forbidden | Authenticated but not the resource owner |
| 404 | Not Found | Resource doesn't exist |
| 500 | Server Error | Unhandled exception (check logs) |

---

**Last Updated:** April 15, 2026
