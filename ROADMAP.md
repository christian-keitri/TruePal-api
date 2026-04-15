# TruePal.Api - Product Roadmap

## Current State (v1.0)

- User registration and login (JWT auth)
- Posts: create, read, update, delete
- Post feeds: recent and trending
- View tracking
- Ownership-based access control
- MVC web UI: landing, dashboard, profile
- 56 unit tests passing

---

## Phase 1: Social Foundation

**Goal:** Make TruePal feel like a social platform. Users can interact with each other's posts.

### Likes
- [ ] `POST /api/posts/{id}/like` - Like a post
- [ ] `DELETE /api/posts/{id}/like` - Unlike a post
- [ ] `GET /api/posts/{id}/likes` - List users who liked
- [ ] Prevent duplicate likes (one per user per post)
- [ ] Update `LikesCount` on Post model
- [ ] Model: `Like` (UserId, PostId, CreatedAt)

### Comments
- [ ] `POST /api/posts/{id}/comments` - Add comment
- [ ] `GET /api/posts/{id}/comments` - List comments (paginated)
- [ ] `DELETE /api/comments/{id}` - Delete own comment
- [ ] Update `CommentsCount` on Post model
- [ ] Model: `Comment` (Id, PostId, UserId, Content, CreatedAt)

### User Profiles
- [ ] `GET /api/users/{id}` - Public profile (username, post count, join date)
- [ ] `PUT /api/users/me` - Update own profile (bio, avatar)
- [ ] Profile avatar upload
- [ ] Model: Add `Bio`, `AvatarUrl` to User

### Follows
- [ ] `POST /api/users/{id}/follow` - Follow a user
- [ ] `DELETE /api/users/{id}/follow` - Unfollow
- [ ] `GET /api/users/{id}/followers` - List followers
- [ ] `GET /api/users/{id}/following` - List following
- [ ] `GET /api/posts/feed` - Posts from followed users
- [ ] Model: `Follow` (FollowerId, FollowingId, CreatedAt)

---

## Phase 2: Engagement & Discovery

**Goal:** Keep users coming back. Notifications, search, richer content.

### Notifications
- [ ] Model: `Notification` (UserId, Type, ReferenceId, Message, IsRead, CreatedAt)
- [ ] Trigger on: new like, new comment, new follower
- [ ] `GET /api/notifications` - List notifications (paginated)
- [ ] `PUT /api/notifications/{id}/read` - Mark as read
- [ ] `PUT /api/notifications/read-all` - Mark all read
- [ ] Unread count in dashboard

### Search
- [ ] `GET /api/posts/search?q=keyword` - Full-text search on content
- [ ] `GET /api/users/search?q=username` - Search users
- [ ] Search by location
- [ ] Search results pagination

### Media
- [ ] Image upload endpoint (`POST /api/media/upload`)
- [ ] Image resizing/thumbnails
- [ ] Storage: local disk (dev) -> cloud storage (prod: S3/Azure Blob)
- [ ] Max file size enforcement
- [ ] Allowed formats: jpg, png, webp

### Pagination
- [ ] Cursor-based pagination for all list endpoints
- [ ] Response format: `{ "data": [...], "nextCursor": "abc123", "hasMore": true }`
- [ ] Replace `count` parameter with `limit` + `cursor`

---

## Phase 3: Safety & Scale

**Goal:** Ready for real users at scale. Moderation, performance, reliability.

### Moderation
- [ ] Report post/user (`POST /api/reports`)
- [ ] Admin role and admin endpoints
- [ ] Block user (`POST /api/users/{id}/block`)
- [ ] Content filtering (profanity, spam detection)
- [ ] Soft delete for posts (hide instead of remove)

### Performance
- [ ] Migrate from SQLite to PostgreSQL
- [ ] Add database indexes (UserId on Posts, composite on Likes/Follows)
- [ ] Response caching for trending/recent feeds
- [ ] Rate limiting on write endpoints
- [ ] Connection pooling

### Real-Time
- [ ] SignalR hub for live notifications
- [ ] Real-time like/comment count updates
- [ ] Online status indicators

### Monitoring
- [ ] Health check endpoint (`GET /api/health`)
- [ ] Structured logging to external service (Seq, Datadog, etc.)
- [ ] Request metrics (response time P50/P95/P99)
- [ ] Error tracking (Sentry or equivalent)
- [ ] Database query performance monitoring

---

## Phase 4: Growth

**Goal:** Features that drive user growth and retention.

### Location Features
- [ ] Nearby posts (`GET /api/posts/nearby?lat=x&lng=y&radius=10km`)
- [ ] Location tagging with coordinates
- [ ] Map view of posts
- [ ] Trending locations

### Rich Content
- [ ] Multiple images per post
- [ ] Link previews (Open Graph)
- [ ] Hashtags with trending topics
- [ ] Mentions (@username)

### Direct Messaging
- [ ] `POST /api/messages` - Send message
- [ ] `GET /api/conversations` - List conversations
- [ ] `GET /api/conversations/{id}/messages` - Message history
- [ ] Real-time messaging via SignalR

### Email
- [ ] Email verification on registration
- [ ] Password reset flow
- [ ] Notification email digests (opt-in)

---

## Principles

1. **Ship Phase 1 first.** Likes + Comments + Follows is the minimum viable social platform.
2. **Each phase builds on the previous.** Don't skip ahead.
3. **Every feature needs tests.** No exceptions (see CODING_STANDARDS.md Rule 48).
4. **Every feature needs an API endpoint.** The MVC views consume the same API the mobile app will.
5. **Performance comes in Phase 3.** Don't optimize prematurely, but don't ignore it either.

---

**Last Updated:** April 15, 2026
