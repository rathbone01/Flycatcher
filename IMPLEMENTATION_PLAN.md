# Flycatcher Chat Application - Comprehensive Implementation Plan

## Overview

This document outlines a phased, team-based implementation plan for delivering two major feature sets to the Flycatcher Blazor Server chat application:

- **Feature Set 1**: Roles & Permissions (7 features)
- **Feature Set 2**: Message & Moderation (6 features)

The plan is organized into 5 phases with clear task dependencies, allowing different team members to work in parallel where possible.

---

## Phase 1: Foundation - Database & Domain Models

**Objective**: Establish the data layer and domain models required for roles/permissions and message moderation.

**Duration**: 3-5 days

### Task 1.1: Create Role Entity and Database Table

**Description**: Create the `Role` entity model to represent server-specific roles with permission flags.

**Details**:
- Create `Flycatcher.Models/Database/Role.cs`
- Fields:
  - `Id` (int, primary key)
  - `ServerId` (int, foreign key to Server)
  - `Name` (string, max 32 chars)
  - `ColorHex` (string, optional, for role color display)
  - `Position` (int, for role hierarchy)
  - `Permissions` (long, bitfield for up to 64 permissions)
  - `CreatedAtUtc` (DateTime)

- Navigation properties:
  - `Server` (one-to-many with Server)
  - `UserRoles` (one-to-many with UserRole)

**Dependencies**: None

**Agent Type**: database-administrator, backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Models, Flycatcher.DataAccess

---

### Task 1.2: Create UserRole Junction Entity

**Description**: Create the `UserRole` entity to manage many-to-many relationship between users and roles within a server (up to 16 roles per user).

**Details**:
- Create `Flycatcher.Models/Database/UserRole.cs`
- Fields:
  - `Id` (int, primary key)
  - `UserId` (int, foreign key)
  - `RoleId` (int, foreign key)
  - `AssignedAtUtc` (DateTime)

- Navigation properties:
  - `User` (foreign key relation)
  - `Role` (foreign key relation)

- Constraints:
  - Unique composite key on (UserId, RoleId)
  - Check constraint to prevent duplicate role assignments

**Dependencies**: Task 1.1

**Agent Type**: database-administrator, backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Models, Flycatcher.DataAccess

---

### Task 1.3: Create Permissions Enumeration

**Description**: Define a comprehensive permissions enumeration used as flags for role permission bitmasks.

**Details**:
- Create `Flycatcher.Services/Enumerations/Permissions.cs`
- Define flags enum with permissions:
  - SendMessages (1 << 0)
  - DeleteOthersMessages (1 << 1)
  - TimeoutUser (1 << 2)
  - BanUser (1 << 3)
  - EditChannels (1 << 4)
  - AddChannels (1 << 5)
  - EditServerSettings (1 << 6)
  - ManageRoles (1 << 7)
  - AssignRoles (1 << 8)

- Include helper methods:
  - `HasPermission(long permissions, Permissions flag)`
  - `AddPermission(long permissions, Permissions flag)`
  - `RemovePermission(long permissions, Permissions flag)`

**Dependencies**: None

**Agent Type**: backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Services

---

### Task 1.4: Extend Message Entity for Soft Delete Support

**Description**: Add fields to the Message entity to support soft delete functionality.

**Details**:
- Modify `Flycatcher.Models/Database/Message.cs`
- Add fields:
  - `DeletedAtUtc` (DateTime?, nullable, default null)
  - `DeletedByUserId` (int?, nullable, foreign key)
  - `IsDeleted` (computed property: `DeletedAtUtc.HasValue`)

- Navigation property:
  - `DeletedByUser` (nullable User relation)

- Update Message queries to exclude soft-deleted messages by default

**Dependencies**: None

**Agent Type**: database-administrator, backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Models, Flycatcher.DataAccess

---

### Task 1.5: Create UserReport Entity

**Description**: Create the `UserReport` entity for site admins to manage user reports and appeals.

**Details**:
- Create `Flycatcher.Models/Database/UserReport.cs`
- Fields:
  - `Id` (int, primary key)
  - `ReportedUserId` (int, foreign key)
  - `ReporterUserId` (int, foreign key, optional - null for internal reports)
  - `Reason` (string, max 500 chars)
  - `Status` (int enum: Open=0, Reviewed=1, Dismissed=2)
  - `CreatedAtUtc` (DateTime)
  - `ReviewedAtUtc` (DateTime?, nullable)
  - `ReviewedByAdminId` (int?, nullable)

- Navigation properties:
  - `ReportedUser` (User relation)
  - `Reporter` (nullable User relation)
  - `ReviewedByAdmin` (nullable User relation)
  - `Appeals` (one-to-many with UserBan)

**Dependencies**: None

**Agent Type**: database-administrator, backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Models, Flycatcher.DataAccess

---

### Task 1.6: Create UserBan Entity

**Description**: Create the `UserBan` entity for tracking permabans and appeals.

**Details**:
- Create `Flycatcher.Models/Database/UserBan.cs`
- Fields:
  - `Id` (int, primary key)
  - `UserId` (int, foreign key)
  - `BannedByAdminId` (int, foreign key)
  - `Reason` (string, max 500 chars)
  - `BannedAtUtc` (DateTime)
  - `AppealStatus` (int enum: None=0, Pending=1, Approved=2, Denied=3)
  - `AppealReason` (string?, nullable, max 500 chars)
  - `AppealSubmittedAtUtc` (DateTime?, nullable)
  - `AppealReviewedAtUtc` (DateTime?, nullable)
  - `AppealReviewedByAdminId` (int?, nullable)

- Navigation properties:
  - `User` (User relation)
  - `BannedByAdmin` (User relation)
  - `AppealReviewedByAdmin` (nullable User relation)

**Dependencies**: None

**Agent Type**: database-administrator, backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Models, Flycatcher.DataAccess

---

### Task 1.7: Create EF Core Migration for Phase 1

**Description**: Create and apply EF Core migration for all new entities and modifications.

**Details**:
- Add DbSet properties to DataContext:
  - `DbSet<Role> Roles`
  - `DbSet<UserRole> UserRoles`
  - `DbSet<UserReport> UserReports`
  - `DbSet<UserBan> UserBans`

- Configure relationships in OnModelCreating:
  - UserRole composite key and relationships
  - UserReport relationships with nullable foreign keys
  - UserBan relationships with optional appeal relationships
  - Message soft delete cascade behavior

- Generate migration:
  ```bash
  dotnet ef migrations add AddRolesPermissionsAndModeration -p Flycatcher.DataAccess
  ```

- Validate migration script for SQL Server compatibility

**Dependencies**: Tasks 1.1, 1.2, 1.4, 1.5, 1.6

**Agent Type**: database-administrator

**Complexity**: M

**Projects Affected**: Flycatcher.DataAccess, Flycatcher.Models

---

### PHASE 1 QA GATE

**Checklist**:
- All entity models compile without errors
- Database migration generates valid SQL
- All foreign key relationships are properly configured
- Composite keys and constraints are correct
- Navigation properties resolve correctly
- No circular dependencies in entity relationships

---

## Phase 2: Backend Services - Business Logic Implementation

**Objective**: Implement core business logic for roles, permissions, message operations, and moderation.

**Duration**: 5-7 days

### Task 2.1: Create Role Management Service

**Description**: Implement service for managing roles (CRUD operations with permission validation).

**Details**:
- Create `Flycatcher.Services/RoleService.cs`
- Methods:
  - `CreateRole(serverId, name, colorHex)` - returns Role with validation
  - `UpdateRole(roleId, name, colorHex, position)` - returns Result
  - `DeleteRole(roleId)` - cascade checks for user assignments
  - `GetRole(roleId)` - returns Role?
  - `GetServerRoles(serverId)` - returns List<Role>
  - `GetUserRoles(serverId, userId)` - returns List<Role>

- Validation:
  - Role name must be 1-32 characters
  - Server can have max 50 roles (suggest, not hard limit)
  - Only users with "ManageRoles" permission can create/edit roles
  - Only users with "AssignRoles" permission can assign roles to users
  - Cannot delete roles that are assigned to users

- Dependencies:
  - IQueryableRepository<Role>
  - IQueryableRepository<UserRole>
  - CallbackService (for role updates)

**Dependencies**: Task 1.1, 1.2, 1.3

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services, Flycatcher.Models

---

### Task 2.2: Create Permission Verification Service

**Description**: Implement service for checking user permissions in servers and channels.

**Details**:
- Create `Flycatcher.Services/PermissionService.cs`
- Methods:
  - `UserHasPermissionAsync(userId, serverId, permission)` - returns bool
  - `UserHasPermissionInChannelAsync(userId, channelId, permission)` - returns bool
  - `CanUserManageUser(managerId, targetUserId, serverId)` - returns bool
  - `GetUserPermissionsInServer(userId, serverId)` - returns long (bitmask)
  - `GetUserPermissionsInChannel(userId, channelId)` - returns long (bitmask)

- Logic:
  - Server owner has all permissions by default
  - Site admins have all permissions everywhere
  - Regular users get permissions from roles
  - Channel-level permissions can override server-level (use AND logic)
  - Combine all roles' permissions via bitwise OR

- Dependencies:
  - IQueryableRepository<UserRole>
  - IQueryableRepository<Role>
  - IQueryableRepository<Channel>
  - IQueryableRepository<Server>
  - IQueryableRepository<UserServer>
  - IQueryableRepository<User>
  - IQueryableRepository<SiteAdmin>

**Dependencies**: Task 1.1, 1.2, 1.3

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services

---

### Task 2.3: Create UserRole Management Service

**Description**: Implement service for assigning and removing roles from users.

**Details**:
- Create `Flycatcher.Services/UserRoleService.cs`
- Methods:
  - `AssignRoleToUser(userId, roleId)` - returns Result
  - `RemoveRoleFromUser(userId, roleId)` - returns Result
  - `ReplaceUserRoles(userId, newRoleIds)` - returns Result (for bulk operations)
  - `GetUserRolesCount(userId, serverId)` - returns int

- Validation:
  - Maximum 16 roles per user per server
  - Cannot assign same role twice
  - Only users with "AssignRoles" permission can perform these operations
  - Validate role belongs to same server as context

- Triggers:
  - Notify via CallbackService when roles change for UI refresh
  - Log role assignments for audit trail (if audit logging exists)

- Dependencies:
  - IQueryableRepository<UserRole>
  - IQueryableRepository<Role>
  - PermissionService (for permission checks)
  - CallbackService

**Dependencies**: Tasks 1.1, 1.2, 2.1, 2.2

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services

---

### Task 2.4: Extend MessageService for Soft Delete and Permission Checks

**Description**: Extend existing MessageService with soft delete and permission validation.

**Details**:
- Modify `Flycatcher.Services/MessageService.cs`
- Add methods:
  - `SoftDeleteMessage(messageId, deletedByUserId)` - returns Result
  - `CanUserDeleteMessage(userId, messageId)` - returns bool
  - `GetChannelMessagesIncludingDeleted(channelId, startIndex, count)` - for admins
  - `PermanentlyDeleteMessage(messageId)` - admin only

- Updates to existing methods:
  - Modify `GetChannelMessages()` to filter out soft-deleted messages
  - Add `DeletedByUserId` parameter to message creation for soft delete tracking
  - Ensure message queries always exclude soft-deleted by default

- Validation:
  - Users can only delete their own messages (unless they have DeleteOthersMessages permission)
  - Message content must still be ≤ 4000 characters on creation
  - Track who deleted the message and when

- Triggers:
  - Notify via CallbackService when message is deleted for UI refresh

- Dependencies:
  - IQueryableRepository<Message>
  - PermissionService
  - CallbackService

**Dependencies**: Task 1.4, 2.2

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services, Flycatcher.Models

---

### Task 2.5: Create User Report Service

**Description**: Implement service for handling user reports and admin review workflow.

**Details**:
- Create `Flycatcher.Services/UserReportService.cs`
- Methods:
  - `ReportUser(reportedUserId, reporterUserId, reason)` - returns Result
  - `GetPendingReports()` - returns List<UserReport>
  - `ReviewReport(reportId, adminId, dismissed)` - returns Result
  - `GetUserReports(userId)` - returns List<UserReport>
  - `GetReportDetails(reportId)` - returns UserReport?

- Validation:
  - Report reason must be 10-500 characters
  - Cannot report yourself
  - Prevent duplicate active reports (same user, same reporter within 24 hours)
  - Only site admins can review reports

- Triggers:
  - Notify admins when new report submitted
  - Log admin review actions

- Dependencies:
  - IQueryableRepository<UserReport>
  - IQueryableRepository<User>
  - CallbackService

**Dependencies**: Task 1.5

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services

---

### Task 2.6: Create User Ban Service

**Description**: Implement service for managing permabans and appeals workflow.

**Details**:
- Create `Flycatcher.Services/UserBanService.cs`
- Methods:
  - `BanUser(userId, adminId, reason)` - returns Result
  - `IsUserBanned(userId)` - returns bool
  - `GetUserBan(userId)` - returns UserBan?
  - `SubmitAppeal(userId, appealReason)` - returns Result
  - `ReviewAppeal(banId, adminId, approved)` - returns Result
  - `GetPendingAppeals()` - returns List<UserBan> (for admins)
  - `UnbanUser(banId, adminId, reason)` - returns Result (emergency unban)

- Validation:
  - Users can only submit one appeal per ban
  - Appeal reason must be 20-500 characters
  - Cannot ban already-banned users (update existing ban instead)
  - Prevent banned users from accessing any server functionality
  - Site admins can review appeals

- Triggers:
  - Notify user when banned
  - Notify admins of pending appeals
  - Notify user when appeal is reviewed

- Dependencies:
  - IQueryableRepository<UserBan>
  - IQueryableRepository<User>
  - CallbackService

**Dependencies**: Task 1.6

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services

---

### Task 2.7: Create Message Validation and Limit Service

**Description**: Implement service for message content validation and character limits.

**Details**:
- Create `Flycatcher.Services/MessageValidationService.cs`
- Constants:
  - `MAX_MESSAGE_LENGTH = 4000`
  - `MIN_MESSAGE_LENGTH = 1`

- Methods:
  - `ValidateMessageContent(content)` - returns Result with validation details
  - `IsMessageContentValid(content)` - returns bool
  - `TruncateMessage(content)` - returns string (truncated to max length)
  - `SanitizeContent(content)` - returns string (basic sanitization if needed)

- Validation rules:
  - Must not be empty or whitespace only
  - Must not exceed 4000 characters
  - Optional: No excessive whitespace repetition
  - Optional: Prevent emoji abuse detection

- Dependencies:
  - None (utility service)

**Dependencies**: None

**Agent Type**: backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Services

---

### Task 2.8: Extend UserService with Ban Checking

**Description**: Add permission and ban checks to existing UserService methods.

**Details**:
- Modify `Flycatcher.Services/UserService.cs`
- Add methods:
  - `CanUserAccessServer(userId, serverId)` - returns bool
  - `GetUserBanStatus(userId)` - returns (isBanned: bool, ban: UserBan?)
  - `GetUserAccessibleServers(userId)` - returns List<Server> (filtered)

- Update existing methods to check:
  - User is not globally banned in critical operations
  - Preserve existing CreateUser, Login methods
  - Add checks before server/channel access

- Triggers:
  - Cache ban status for performance

- Dependencies:
  - IQueryableRepository<UserBan>
  - UserBanService
  - CallbackService

**Dependencies**: Task 2.6

**Agent Type**: backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Services

---

### Task 2.9: Create CallbackIdGenerator Extensions

**Description**: Add new callback types and extend CallbackIdGenerator for new features.

**Details**:
- Modify `Flycatcher.Services/Enumerations/CallbackType.cs` (if it exists) or create it
- Add new enum values:
  - `RolesUpdated`
  - `UserRoleChanged`
  - `MessageDeleted`
  - `MessageModified`
  - `UserBanned`
  - `UserReportSubmitted`
  - `ReportReviewRequested`
  - `AppealSubmitted`
  - `AppealReviewRequested`

- Extend `Flycatcher.Services/CallbackIdGenerator.cs`
- Add methods:
  - Methods for generating IDs for each callback type
  - Generic pattern for (Type, Id) generation

**Dependencies**: Tasks 1.1, 1.5, 1.6

**Agent Type**: backend-developer

**Complexity**: S

**Projects Affected**: Flycatcher.Services

---

### PHASE 2 QA GATE

**Checklist**:
- All services compile without errors
- Dependency injection wiring verified
- Permission logic passes unit tests for edge cases
- Soft delete queries correctly exclude deleted messages
- Ban and appeal workflows verified
- No unauthorized permission elevation possible
- All services use async/await correctly
- CallbackService integration verified

**Unit Tests Required**:
- PermissionService: 10+ test cases
- RoleService: 8+ test cases
- UserRoleService: 8+ test cases
- MessageService (soft delete): 6+ test cases
- UserBanService: 10+ test cases

---

## Phase 3: Backend Validation & Authorization Layer

**Objective**: Implement validation middleware and authorization checks for API endpoints.

**Duration**: 3-4 days

### Task 3.1: Create Server-Level Permission Attributes

**Description**: Create custom authorization attributes for server-specific permission checks.

**Details**:
- Create `Flycatcher.Services/Attributes/RequirePermissionAttribute.cs`
- Parameters:
  - `permission` (Permissions enum)
  - `requireServerOwner` (bool, optional)
  - `allowSiteAdmin` (bool, default true)

- Implementation:
  - Can be applied to component methods
  - Checks permission before method execution
  - Returns error result if unauthorized
  - Works with Blazor component lifecycle

- Usage example:
  ```csharp
  [RequirePermission(Permissions.ManageRoles)]
  public async Task CreateRole(string name) { ... }
  ```

- Dependencies:
  - PermissionService
  - UserStateService

**Dependencies**: Tasks 2.1, 2.2

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services

---

### Task 3.2: Create Channel-Level Permission Validation Service

**Description**: Implement detailed channel-level permission validation with role inheritance.

**Details**:
- Create `Flycatcher.Services/ChannelPermissionService.cs`
- Methods:
  - `CanUserAccessChannel(userId, channelId)` - returns bool
  - `CanUserSendMessageInChannel(userId, channelId)` - returns bool
  - `CanUserDeleteMessageInChannel(userId, channelId)` - returns bool
  - `GetChannelEffectivePermissions(userId, channelId)` - returns long
  - `GetChannelOverrides(channelId)` - returns channel-specific role permissions

- Logic:
  - Inherit server-level permissions as base
  - Apply channel-level overrides if they exist
  - Check role visibility in channel
  - Enforce channel-specific talk permissions

- Database schema consideration:
  - May need ChannelRoleOverride entity (future enhancement)
  - For MVP, use server roles directly

- Dependencies:
  - IQueryableRepository<Channel>
  - IQueryableRepository<UserRole>
  - PermissionService

**Dependencies**: Tasks 2.2, 1.4

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services

---

### Task 3.3: Create Message Operation Authorization Service

**Description**: Implement comprehensive authorization checks for message operations.

**Details**:
- Create `Flycatcher.Services/MessageAuthorizationService.cs`
- Methods:
  - `CanUserSendMessage(userId, channelId)` - returns Result
  - `CanUserDeleteMessage(userId, messageId)` - returns Result
  - `CanUserViewMessage(userId, messageId)` - returns Result
  - `CanAdminViewDeletedMessages(userId)` - returns bool

- Validation:
  - Check channel access first
  - Check SendMessages permission
  - Check message doesn't exceed 4000 chars
  - Check user is not timed out or banned
  - Verify message ownership for self-delete
  - Check DeleteOthersMessages permission for admin delete

- Return Results with detailed error messages for UI

- Dependencies:
  - IQueryableRepository<Message>
  - IQueryableRepository<Channel>
  - ChannelPermissionService
  - UserBanService
  - MessageValidationService

**Dependencies**: Tasks 2.4, 2.7, 3.2

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services

---

### Task 3.4: Extend SiteAdminService with Ban/Report Management

**Description**: Add ban and report management methods to existing SiteAdminService.

**Details**:
- Modify `Flycatcher.Services/SiteAdminService.cs` (or create if missing)
- Add methods:
  - `GetAllUserBans()` - returns List<UserBan>
  - `GetAllPendingReports()` - returns List<UserReport>
  - `GetUserBanHistory(userId)` - returns List<UserBan>
  - `GetUserReportHistory(userId)` - returns List<UserReport>
  - `IsSiteAdmin(userId)` - returns bool

- Ensure only site admins can call these methods
- Add audit logging for sensitive operations

- Dependencies:
  - IQueryableRepository<UserBan>
  - IQueryableRepository<UserReport>
  - IQueryableRepository<SiteAdmin>
  - CallbackService

**Dependencies**: Tasks 2.5, 2.6

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services

---

### Task 3.5: Create Timeout User Service (Optional Enhancement)

**Description**: Create service for temporary user timeouts (complement to permanent bans).

**Details**:
- Create `Flycatcher.Services/UserTimeoutService.cs`
- Methods:
  - `TimeoutUser(userId, serverId, durationMinutes, reason)` - returns Result
  - `IsUserTimedOut(userId, serverId)` - returns bool
  - `GetUserTimeout(userId, serverId)` - returns UserTimeout?
  - `RemoveTimeout(userId, serverId)` - returns Result

- Note: Requires new UserTimeout entity (not in Phase 1)
- For MVP: Track in UserTimeout entity with ExpiresAtUtc
- Affects SendMessages permission check

- Database schema (defer if needed):
  - Id, UserId, ServerId, Reason, TimeoutAtUtc, ExpiresAtUtc

- Dependencies:
  - IQueryableRepository<UserTimeout>
  - CallbackService

**Dependencies**: Deferred - create UserTimeout entity first

**Agent Type**: backend-developer

**Complexity**: M

**Projects Affected**: Flycatcher.Services, Flycatcher.Models (future)

---

### PHASE 3 QA GATE

**Checklist**:
- All authorization attributes work correctly
- Permission inheritance logic verified
- No authorization bypass possible
- Message length validation enforced
- Ban checking prevents access appropriately
- Site admin checks on sensitive operations
- All services return appropriate error messages
- Unit tests for authorization (15+ cases)

**Authorization Test Matrix**:
- Server owner vs regular user permissions
- Site admin universal access
- Role-based permission combinations
- Channel-level overrides
- Banned user access denial
- Timeout effect on messaging

---

## Phase 4: Frontend Components & UI Implementation

**Objective**: Create Blazor components for role management, message operations, and moderation UI.

**Duration**: 6-8 days

### Task 4.1: Create Role Management Component

**Description**: Build UI component for creating, editing, and deleting roles.

**Details**:
- Create `Flycatcher/Components/Modals/RoleManagementDialog.razor`
- Create `Flycatcher/Components/Modals/CreateRoleDialog.razor`
- Create `Flycatcher/Components/Modals/EditRoleDialog.razor`

**Features**:
- List all server roles in a data grid (MudBlazor)
- Create new role with:
  - Name input (validation: 1-32 chars)
  - Color picker for role color
  - Position selector for hierarchy
  - Permission checkboxes (all 9 permissions)

- Edit role with:
  - Same inputs as create
  - List of users assigned to role
  - Warning if deleting role with assignments

- Delete role with confirmation

- Show number of users assigned to each role

- Real-time update via CallbackService (RolesUpdated)

**Components**:
- MudDialog for modal
- MudDataGrid for role list
- MudCheckBox for permissions
- MudColorPicker for role color
- MudButton for actions

**Dependencies**:
- RoleService
- PermissionService
- UserRoleService
- CallbackService
- UserStateService

**Dependencies**: Tasks 2.1, 2.2, 2.3

**Agent Type**: frontend-developer, fullstack-developer

**Complexity**: M

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### Task 4.2: Create Role Assignment Component

**Description**: Build UI for assigning roles to users in user context menu.

**Details**:
- Create `Flycatcher/Components/Widgets/RoleAssignmentPopover.razor`
- Create `Flycatcher/Components/Modals/AssignRoleDialog.razor`

**Features**:
- Display in right sidebar user list context menu
- Show "Manage Roles" option for authorized users
- Dialog listing:
  - All available roles for server
  - Checkboxes for current user role assignments
  - Show current count (X/16 roles)
  - Display role colors

- Allow bulk assignment/removal up to 16 roles

- Validation:
  - Check user has AssignRoles permission
  - Prevent exceeding 16 roles
  - Show permission denied message if unauthorized

- Real-time feedback via CallbackService (UserRoleChanged)

**Components**:
- MudPopover for context menu
- MudChips for role display
- MudCheckBox for selection
- MudButton for apply/cancel

**Dependencies**:
- UserRoleService
- PermissionService
- CallbackService
- UserStateService

**Dependencies**: Tasks 2.1, 2.3, 3.1

**Agent Type**: frontend-developer, fullstack-developer

**Complexity**: M

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### Task 4.3: Extend Message Display with Soft Delete UI

**Description**: Update message display to show soft-deleted messages with appropriate styling.

**Details**:
- Modify `Flycatcher/Components/Widgets/MessageDisplay.razor` (or similar)
- Add features:
  - Show deleted messages with strikethrough/grayed text: "[Message deleted]"
  - Show who deleted it (if available): "Deleted by [Admin] - [timestamp]"
  - Hide deleted message content except to admins
  - Add delete button in message context menu
  - Soft delete when clicked (not hard delete)

- Styling:
  - Deleted messages: reduced opacity, strikethrough
  - Admin view: show content with delete marker
  - Show "This message was deleted" note

**Permissions**:
- Show delete option only if user can delete
- Only show full content of deleted messages to admins
- Role color display in message author name

**Components**:
- Message item with conditional rendering
- Context menu for delete action
- Timestamp display in user local time (existing)

**Triggers**:
- Listen for MessageDeleted callback
- Refresh message display when callback fires

**Dependencies**:
- MessageService
- MessageAuthorizationService
- PermissionService
- CallbackService

**Dependencies**: Tasks 2.4, 3.3

**Agent Type**: frontend-developer, fullstack-developer

**Complexity**: M

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### Task 4.4: Create User Report Dialog Component

**Description**: Build UI for reporting users with reason form.

**Details**:
- Create `Flycatcher/Components/Modals/ReportUserDialog.razor`

**Features**:
- Triggered from user context menu
- Form fields:
  - User display (read-only)
  - Report reason textarea (10-500 chars)
  - Submit button

- Validation:
  - Cannot report yourself
  - Reason length validation
  - Prevent duplicate recent reports

- Success message: "User reported. Thank you for helping keep our community safe."
- Error handling for duplicate reports

**Components**:
- MudDialog
- MudTextField for reason
- MudButton for submit

**Dependencies**:
- UserReportService
- UserStateService

**Dependencies**: Task 2.5

**Agent Type**: frontend-developer, fullstack-developer

**Complexity**: S

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### Task 4.5: Create Admin Reports Dashboard Component

**Description**: Build dashboard for site admins to review user reports.

**Details**:
- Create `Flycatcher/Components/Pages/AdminReportsDashboard.razor`

**Features**:
- List all pending reports in data grid (MudDataGrid)
- Columns:
  - Reported user
  - Reporter (if available)
  - Reason
  - Created date
  - Status
  - Actions

- Actions for each report:
  - View full details
  - Dismiss report
  - Ban user (with additional reason)
  - Open chat with reporter (if needed)

- Filter/sort by:
  - Status (pending/reviewed/dismissed)
  - Created date
  - Reported user

- Statistics:
  - Total pending reports
  - Total reviewed this month
  - Most reported users

**Access Control**:
- Only visible to site admins
- Redirect if not admin

**Components**:
- MudDataGrid for report list
- MudDialog for details view
- MudButton for actions
- MudChip for status display

**Dependencies**:
- UserReportService
- UserBanService
- SiteAdminService
- PermissionService
- UserStateService

**Dependencies**: Tasks 2.5, 2.6, 3.4

**Agent Type**: frontend-developer, fullstack-developer

**Complexity**: M

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### Task 4.6: Create Ban & Appeal Management Component

**Description**: Build UI for site admins to manage bans and review appeals.

**Details**:
- Create `Flycatcher/Components/Pages/AdminBansDashboard.razor`
- Create `Flycatcher/Components/Modals/BanUserDialog.razor`
- Create `Flycatcher/Components/Modals/ApproveAppealDialog.razor`

**Features**:
- Ban management:
  - List all active bans
  - Ban date, reason, banned by admin
  - View appeal status if present
  - Option to unban (emergency)

- Appeal management:
  - List pending appeals (separate tab or section)
  - Show appeal reason
  - Show original ban reason
  - Approve/Deny buttons with reason field
  - Show all appeals history

- Ban user dialog:
  - Username (read-only or lookup)
  - Reason input (required)
  - Confirmation before ban

- Approve/Deny appeal dialog:
  - Show appeal text
  - Decision field
  - Reason for decision
  - Optional: unban user if approved

**Statistics**:
- Total active bans
- Pending appeals count
- Appeals reviewed this month
- Ban rate trends

**Access Control**:
- Only visible to site admins

**Components**:
- MudTabs for Ban/Appeal tabs
- MudDataGrid for lists
- MudDialog for actions
- MudAlert for confirmation

**Dependencies**:
- UserBanService
- SiteAdminService
- UserService
- PermissionService
- UserStateService

**Dependencies**: Tasks 2.6, 3.4

**Agent Type**: frontend-developer, fullstack-developer

**Complexity**: M

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### Task 4.7: Create User Appeal Submission Component

**Description**: Build UI for banned users to submit appeals.

**Details**:
- Create `Flycatcher/Components/Modals/SubmitBanAppealDialog.razor`

**Features**:
- Show on login/app start if user is banned
- Display:
  - Ban reason
  - Ban date
  - Who banned them (optional)
  - "Submit an Appeal" section

- Appeal form:
  - Reason textarea (20-500 chars)
  - Only allow one appeal per ban
  - Submit button
  - "You have submitted an appeal. An admin will review it soon."

- Show appeal status if already submitted:
  - "Your appeal is pending review"
  - "Your appeal was approved. You can now access the app." (unban)
  - "Your appeal was denied: [admin reason]"

**Access Control**:
- Show to banned users
- Disable submission if already submitted

**Components**:
- MudAlert for ban information
- MudTextField for appeal reason
- MudButton for submit
- MudProgressLinear for status

**Dependencies**:
- UserBanService
- UserStateService

**Dependencies**: Task 2.6

**Agent Type**: frontend-developer, fullstack-developer

**Complexity**: M

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### Task 4.8: Create Role Color Display in User Names

**Description**: Update user name displays to show role color.

**Details**:
- Modify message author name display
- Modify user list display in right sidebar
- Show highest role's color as text color or background

**Features**:
- Get user's roles in server
- Display primary role color (first/highest priority role)
- Apply to text or background based on design
- Show role name as tooltip on hover
- Fallback to default color if no role color set

**Components**:
- Update MessageAuthor component
- Update UserListItem component
- MudTooltip for role name

**Dependencies**:
- UserRoleService
- RoleService

**Dependencies**: Tasks 2.1, 2.3

**Agent Type**: frontend-developer, fullstack-developer

**Complexity**: S

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### Task 4.9: Add Message Character Counter to Compose Box

**Description**: Add UI feedback for message length validation.

**Details**:
- Modify message compose component
- Add character counter: "1234/4000"
- Add visual indicator (progress bar or color change) as limit approached
- Disable send button when message is empty or exceeds 4000 chars
- Show error message if limit exceeded

**Features**:
- Real-time character count
- Color change at 80% (3200 chars) - yellow
- Color change at 100% (4000 chars) - red
- Tooltip showing character limit

**Components**:
- MudTextField with counter
- MudLinearProgress for visual indicator
- Styling for alerts

**Dependencies**:
- MessageValidationService

**Dependencies**: Task 2.7

**Agent Type**: frontend-developer

**Complexity**: S

**Projects Affected**: Flycatcher, Flycatcher.Services

---

### PHASE 4 QA GATE

**Checklist**:
- All components render without errors
- Role management CRUD fully functional
- Message delete UI works with soft delete backend
- Report submission validates input correctly
- Admin dashboards load data and respond to actions
- Ban appeal flow works end-to-end
- Role colors display correctly
- Message character counter prevents exceeding limit
- CallbackService integration triggers UI updates
- No XSS vulnerabilities in user input display
- Responsive design verified
- Accessibility checks (keyboard nav, screen readers)

**UI/UX Testing**:
- Permission denied states handled gracefully
- Loading states shown for async operations
- Error messages clear and actionable
- Confirmation dialogs for destructive actions
- Mobile responsiveness verified

---

## Phase 5: Integration, Testing & Deployment

**Objective**: Complete end-to-end testing, fix integration issues, and prepare for production deployment.

**Duration**: 4-6 days

### Task 5.1: Create Comprehensive Unit Tests

**Description**: Write unit tests for all new services with >80% code coverage.

**Details**:
- Create test files in `Flycatcher.Tests/`:
  - `RoleServiceTests.cs` - 15+ test cases
  - `PermissionServiceTests.cs` - 20+ test cases
  - `UserRoleServiceTests.cs` - 12+ test cases
  - `MessageServiceTests.cs` - 10+ test cases (soft delete)
  - `UserBanServiceTests.cs` - 15+ test cases
  - `UserReportServiceTests.cs` - 12+ test cases
  - `MessageAuthorizationServiceTests.cs` - 15+ test cases

**Test Coverage**:
- Happy path scenarios
- Edge cases (boundary values, max/min)
- Error scenarios
- Permission elevation attempts
- Concurrent operations
- Null/empty input handling

**Testing Framework**: xUnit (or existing framework)

**Dependencies**: All backend services (Phase 2-3)

**Agent Type**: test-automator, qa-expert

**Complexity**: M

**Projects Affected**: Flycatcher.Tests

---

### Task 5.2: Create Integration Tests

**Description**: Write integration tests for end-to-end workflows.

**Details**:
- Create test files:
  - `RoleWorkflowIntegrationTests.cs` - Create role → Assign to users → Delete
  - `MessageDeleteWorkflowTests.cs` - Create → Delete → Verify UI update
  - `BanAppealWorkflowTests.cs` - Ban user → Submit appeal → Review
  - `PermissionEnforcementTests.cs` - Multi-user permission scenarios
  - `CallbackServiceIntegrationTests.cs` - Real-time update propagation

**Test Scenarios**:
- User creates role, assigns to 3 users, modifies permissions, deletes role
- User sends message, deletes it, other users see deletion
- Admin reports user, reviews report, bans user, user appeals, admin approves
- Multiple users with different roles perform actions, verify permissions
- Rapid permission changes, verify UI updates correctly

**Database Setup**:
- Use test database or in-memory EF Core
- Seed test data before each test
- Clean up after tests

**Dependencies**: All services and components

**Agent Type**: test-automator, qa-expert

**Complexity**: M

**Projects Affected**: Flycatcher.Tests

---

### Task 5.3: Create UI Component Tests

**Description**: Test Blazor components for functionality and user interactions.

**Details**:
- Create Blazor component tests using bUnit library
- Test components:
  - `RoleManagementDialog` - Create, edit, delete flows
  - `RoleAssignmentPopover` - Assignment limit, permission checks
  - `ReportUserDialog` - Form validation, submission
  - `AdminReportsDashboard` - Data loading, filtering, actions
  - `AdminBansDashboard` - Ban list, appeal review
  - `MessageDisplay` - Delete button visibility, soft delete display

**Test Coverage**:
- Component renders correctly
- User interactions (clicks, form inputs) work
- Validation messages display
- Permission-based UI elements show/hide
- Callbacks trigger UI updates

**Testing Framework**: bUnit

**Dependencies**: Phase 4 components

**Agent Type**: test-automator, qa-expert

**Complexity**: M

**Projects Affected**: Flycatcher.Tests

---

### Task 5.4: Security Audit & Authorization Testing

**Description**: Comprehensive security review focused on authorization and data access.

**Details**:
- Test authorization enforcement:
  - Non-admin cannot access admin pages
  - Users cannot manage others' roles without permission
  - Banned users cannot access any functionality
  - Private data filtering (only see own messages, accessible servers)

- Test permission elevation attempts:
  - Modify request to add self-permission
  - Delete roles assigned to attacker
  - Access other user's appeal

- Test data isolation:
  - User A cannot see User B's messages in different servers
  - User A cannot see User B's friend list
  - Reports visible only to admins

- Test SQL injection/XSS prevention:
  - Special characters in role names
  - HTML/script in report reasons
  - Long input strings

- Generate security report with findings and fixes

**Dependencies**: All features (Phase 2-4)

**Agent Type**: security-engineer, qa-expert

**Complexity**: M

**Projects Affected**: All projects

---

### Task 5.5: Performance & Load Testing

**Description**: Test system performance under load.

**Details**:
- Load testing scenarios:
  - 100 concurrent users, 10 messages/second
  - Role permission checks with 1000 users and 100 roles
  - Large message lists (10,000+ messages)
  - Rapid callback notifications

- Performance metrics to track:
  - Message creation latency (target: <100ms)
  - Permission check latency (target: <50ms)
  - UI update time from callback (target: <200ms)
  - Database query performance
  - Memory usage

- Optimize bottlenecks found:
  - Add caching for roles/permissions
  - Index database queries
  - Batch callback notifications

- Generate performance baseline report

**Dependencies**: All features

**Agent Type**: qa-expert, backend-developer, performance-monitor

**Complexity**: M

**Projects Affected**: All projects

---

### Task 5.6: Create Database Migration Validation

**Description**: Validate all migrations work correctly on fresh and upgraded databases.

**Details**:
- Test migration script:
  - Fresh database installation
  - Upgrade from current schema
  - Rollback and reapply
  - Verify all indexes present
  - Verify all constraints present

- Test data preservation:
  - Create existing data
  - Run migration
  - Verify data integrity
  - Check foreign key relationships

- Generate migration documentation:
  - What changed
  - Potential issues
  - Rollback procedure
  - Estimated duration

- Test on SQL Server 2019+ (production target)

**Dependencies**: Phase 1 (Database schema)

**Agent Type**: database-administrator

**Complexity**: M

**Projects Affected**: Flycatcher.DataAccess

---

### Task 5.7: Create Feature Documentation

**Description**: Write comprehensive documentation for new features.

**Details**:
- Create documentation files:
  - `FEATURES_ROLES_PERMISSIONS.md` - Role management guide
  - `FEATURES_MESSAGE_MODERATION.md` - Message delete and moderation
  - `ADMIN_DASHBOARD_GUIDE.md` - How to use admin dashboards
  - `API_CHANGELOG.md` - New service methods and breaking changes

**Documentation includes**:
- Feature overview
- User workflows with screenshots/videos
- Admin procedures
- Troubleshooting guide
- API reference for developers
- Database schema changes

**Dependencies**: All completed features

**Agent Type**: project-manager, ui-designer

**Complexity**: S

**Projects Affected**: Documentation

---

### Task 5.8: Code Review & Refactoring

**Description**: Comprehensive code review across all new features with refactoring.

**Details**:
- Code review checklist:
  - Naming conventions consistency
  - Code duplication (DRY)
  - Proper error handling
  - Async/await best practices
  - Repository pattern usage
  - Dependency injection configuration
  - XML documentation completeness
  - Architecture adherence

- Refactoring opportunities:
  - Extract common validation logic
  - Consolidate similar service methods
  - Improve error message consistency
  - Optimize permission check queries

- Review checklist:
  - All methods have XML docs
  - No magic numbers (use constants)
  - Proper validation at service boundary
  - Consistent Result/exception handling
  - No unnecessary async operations
  - Proper null checks and guards

- Generate code review report with action items

**Dependencies**: All code (Phase 2-4)

**Agent Type**: code-reviewer, feature-dev:code-architect

**Complexity**: M

**Projects Affected**: All projects

---

### Task 5.9: Create Build & Deployment Verification

**Description**: Verify application builds and deploys correctly.

**Details**:
- Build verification:
  ```bash
  dotnet clean
  dotnet build
  dotnet test
  ```

- Verify no warnings or errors
- Check test coverage reports
- Verify assembly versions updated

- Deployment verification:
  - Test deployment to staging environment
  - Run smoke tests post-deployment
  - Verify database migration applied
  - Test all new features in deployed environment
  - Check performance metrics in production environment

- Create deployment checklist:
  - Database backups
  - Migration scripts ready
  - Rollback plan
  - User communication plan
  - Monitoring setup for new features

- Generate build report with:
  - Assembly versions
  - Test results
  - Code coverage
  - Build duration
  - Any warnings or deprecations

**Dependencies**: All features complete

**Agent Type**: qa-expert, project-manager

**Complexity**: M

**Projects Affected**: All projects

---

### Task 5.10: Create Release Notes & Changelog

**Description**: Document all changes for user and developer communication.

**Details**:
- Create `RELEASE_NOTES.md` with:
  - Feature Set 1: Roles & Permissions
    - 7 features with descriptions
    - Screenshots or GIFs for UI changes
    - New permissions explained

  - Feature Set 2: Message & Moderation
    - 6 features with descriptions
    - Workflow diagrams for ban/appeal
    - Data retention policy for deleted messages

  - Breaking changes (if any)
  - Database schema changes
  - Migration instructions
  - Known issues
  - Performance improvements
  - Future roadmap

- Update `CHANGELOG.md`:
  - Version number
  - Release date
  - Summary of changes
  - Link to detailed release notes

- Create admin communication:
  - Email template announcing new moderation features
  - Training guide for ban/appeal process
  - FAQ for common issues

**Dependencies**: All features complete

**Agent Type**: project-manager

**Complexity**: S

**Projects Affected**: Documentation

---

### PHASE 5 QA GATE

**Checklist**:
- All unit tests passing (>80% coverage)
- All integration tests passing
- UI component tests passing
- Security audit completed, no critical findings
- Performance baseline established
- Database migrations validated
- Code review completed, no outstanding issues
- Build succeeds without warnings
- Documentation complete
- Release notes written and reviewed
- Deployment verification successful
- No known critical bugs

**Final Sign-Off**:
- Tech lead: Code quality and architecture
- QA lead: Test coverage and quality
- Security lead: Authorization and data protection
- Product: Feature completeness and UX
- DevOps: Deployment readiness

---

## Cross-Phase Considerations

### Team Coordination
- **Parallel Work**: Phases 2 and 4 can overlap (one team on backend, one on frontend)
- **Dependency Management**: Phase 3 depends on Phase 2 completion
- **Daily Standups**: Track blockers and dependencies
- **Code Reviews**: Each task should have peer review before merge

### Quality Gates Between Phases
- **After Phase 1**: Database validated, migrations tested
- **After Phase 2**: Services unit tested, no authorization bypass possible
- **After Phase 3**: Authorization attributes tested, permission logic verified
- **After Phase 4**: Components functional, UI tested
- **After Phase 5**: Everything integrated and production-ready

### Communication Protocol
```json
{
  "agent": "task-distributor",
  "status": "task_assignment",
  "payload": {
    "task_id": "1.1",
    "task_name": "Create Role Entity and Database Table",
    "assigned_agent_type": "database-administrator",
    "priority": "high",
    "dependencies": [],
    "estimated_hours": 2,
    "deadline": "2026-02-27"
  }
}
```

### Progress Tracking Template
```json
{
  "phase": 1,
  "completed_tasks": 7,
  "in_progress_tasks": 1,
  "total_tasks": 7,
  "completion_percentage": 100,
  "blockers": [],
  "risks": []
}
```

---

## Task Distribution Strategy

### Phase 1: Database & Models (3-5 days)
**Optimal Team Composition**: 1-2 backend developers + 1 database administrator

- **Days 1-2**: Tasks 1.1-1.3 (parallel)
  - Developer 1: Tasks 1.1, 1.2 (entities)
  - Developer 2: Task 1.3 (enumerations)

- **Day 2-3**: Tasks 1.4-1.6 (parallel)
  - Developer 1: Task 1.4 (Message extension)
  - Developer 2: Tasks 1.5, 1.6 (Report/Ban entities)

- **Day 3-4**: Task 1.7 (migration)
  - DBA: Create and validate migration

- **Day 4-5**: Phase 1 QA Gate + fixes

---

### Phase 2: Backend Services (5-7 days)
**Optimal Team Composition**: 2-3 backend developers

- **Days 1-2**: Tasks 2.1-2.3 (parallel) + Task 2.9
  - Developer 1: Tasks 2.1, 2.3 (Role services)
  - Developer 2: Tasks 2.2, 2.9 (Permission service + enumerations)

- **Days 2-4**: Tasks 2.4-2.8 (some parallel)
  - Developer 1: Tasks 2.4, 2.7 (Message operations)
  - Developer 2: Tasks 2.5, 2.6, 2.8 (Ban/Report services)

- **Days 4-5**: Task 3.5 (optional timeout service) + Phase 2 unit tests
  - Developer 3: Unit tests (5.1)

- **Days 5-7**: Phase 2 QA Gate + test coverage improvement

---

### Phase 3: Authorization Layer (3-4 days)
**Optimal Team Composition**: 1-2 backend developers

- **Days 1-2**: Tasks 3.1-3.3 (parallel)
  - Developer 1: Tasks 3.1, 3.2 (Attributes, channel permissions)
  - Developer 2: Task 3.3 (Message authorization)

- **Days 2-3**: Tasks 3.4-3.5
  - Developer 1: Task 3.4 (SiteAdminService extension)
  - Developer 2: Task 3.5 (Optional timeout service)

- **Days 3-4**: Phase 3 QA Gate + integration with Phase 2

---

### Phase 4: Frontend Components (6-8 days)
**Optimal Team Composition**: 2-3 frontend developers

- **Days 1-2**: Tasks 4.1-4.2 (parallel)
  - Frontend 1: Task 4.1 (Role management UI)
  - Frontend 2: Task 4.2 (Role assignment UI)

- **Days 2-4**: Tasks 4.4-4.7 (parallel)
  - Frontend 1: Tasks 4.4, 4.7 (Report dialog, appeal UI)
  - Frontend 2: Tasks 4.5, 4.6 (Admin dashboards)

- **Days 4-5**: Tasks 4.3, 4.8, 4.9 (message updates)
  - Frontend 1: Task 4.3 (Message display soft delete)
  - Frontend 2: Tasks 4.8, 4.9 (Role colors, character counter)

- **Days 5-8**: Phase 4 QA Gate + UI testing

---

### Phase 5: Testing & Deployment (4-6 days)
**Optimal Team Composition**: 1-2 QA engineers + 1 security engineer + 1 DevOps/DBA

- **Days 1-2**: Tasks 5.1-5.3 (parallel)
  - QA 1: Tasks 5.1, 5.2 (Unit and integration tests)
  - QA 2: Task 5.3 (UI component tests)
  - QA 3: Task 5.4 (Security audit)

- **Days 2-3**: Tasks 5.5-5.6 (parallel)
  - QA: Task 5.5 (Performance testing)
  - DBA: Task 5.6 (Migration validation)

- **Days 3-4**: Tasks 5.7-5.10
  - DevOps: Task 5.9 (Build verification)
  - PM: Tasks 5.7, 5.10 (Documentation, release notes)

- **Days 4-6**: Phase 5 QA Gate + final sign-off

---

## Dependency Map (Simplified)

```
Phase 1: Foundation
├── 1.1: Role Entity
├── 1.2: UserRole Entity (depends on 1.1)
├── 1.3: Permissions Enum
├── 1.4: Message Extension
├── 1.5: UserReport Entity
├── 1.6: UserBan Entity
└── 1.7: Migration (depends on 1.1-1.6)

Phase 2: Backend Services
├── 2.1: RoleService (depends on 1.1, 1.2, 1.3, 1.7)
├── 2.2: PermissionService (depends on 1.1, 1.2, 1.3)
├── 2.3: UserRoleService (depends on 1.1, 1.2, 2.1, 2.2)
├── 2.4: MessageService Extension (depends on 1.4, 2.2)
├── 2.5: UserReportService (depends on 1.5)
├── 2.6: UserBanService (depends on 1.6)
├── 2.7: MessageValidationService
├── 2.8: UserService Extension (depends on 2.6)
└── 2.9: CallbackId Generator (depends on 1.1, 1.5, 1.6)

Phase 3: Authorization
├── 3.1: Permission Attributes (depends on 2.1, 2.2)
├── 3.2: ChannelPermissionService (depends on 2.2, 1.4)
├── 3.3: MessageAuthorizationService (depends on 2.4, 2.7, 3.2)
├── 3.4: SiteAdminService Extension (depends on 2.5, 2.6)
└── 3.5: TimeoutService (optional)

Phase 4: Frontend
├── 4.1: Role Management (depends on 2.1, 2.2, 2.3)
├── 4.2: Role Assignment (depends on 2.1, 2.3, 3.1)
├── 4.3: Message Delete UI (depends on 2.4, 3.3)
├── 4.4: Report Dialog (depends on 2.5)
├── 4.5: Reports Dashboard (depends on 2.5, 2.6, 3.4)
├── 4.6: Bans Dashboard (depends on 2.6, 3.4)
├── 4.7: Appeal Dialog (depends on 2.6)
├── 4.8: Role Colors (depends on 2.1, 2.3)
└── 4.9: Message Counter (depends on 2.7)

Phase 5: Testing & Deployment
├── 5.1: Unit Tests (depends on Phase 2-3)
├── 5.2: Integration Tests (depends on Phase 2-4)
├── 5.3: UI Tests (depends on Phase 4)
├── 5.4: Security Audit (depends on Phase 2-4)
├── 5.5: Performance Tests (depends on Phase 2-4)
├── 5.6: Migration Validation (depends on Phase 1)
├── 5.7: Documentation (depends on Phase 2-4)
├── 5.8: Code Review (depends on Phase 2-4)
├── 5.9: Build Verification (depends on all)
└── 5.10: Release Notes (depends on all)
```

---

## Risk Assessment & Mitigation

### High-Risk Areas

**Risk 1: Permission Logic Complexity**
- Impact: Authorization bypass, privilege escalation
- Mitigation:
  - Unit test all permission combinations (20+ cases)
  - Security audit before Phase 5
  - Code review by 2+ senior developers
  - Formal authorization test matrix

**Risk 2: Data Consistency with Soft Delete**
- Impact: Orphaned references, reporting issues
- Mitigation:
  - Explicit testing of soft delete queries
  - Database constraints validation
  - Integration tests for complex queries
  - Migration testing on production-like data

**Risk 3: Real-time Update Delays**
- Impact: Stale UI, confused users
- Mitigation:
  - CallbackService integration tests
  - Load testing with 100+ concurrent users
  - Performance profiling
  - Message queue optimization if needed

**Risk 4: Admin Dashboard Performance**
- Impact: Timeouts when listing large reports/bans
- Mitigation:
  - Pagination/lazy loading
  - Database indexing on filtered columns
  - Caching for dashboard statistics
  - Load testing

### Medium-Risk Areas

**Risk 5: Role Color Accessibility**
- Impact: Users with color blindness cannot distinguish roles
- Mitigation:
  - Use role name alongside color
  - Icon or symbol with color
  - Accessibility testing with color-blind simulator

**Risk 6: Message Character Limit Edge Cases**
- Impact: Users cannot send important messages
- Mitigation:
  - Test with various Unicode characters
  - Test 4000-char edge case
  - Clear error messages

---

## Success Criteria

### Phase 1 Success
- All 7 database-related tasks completed
- Migration applies cleanly
- No data integrity issues
- Phase 1 QA gate passed

### Phase 2 Success
- All 9 service tasks completed
- >80% unit test coverage
- No authorization bugs found
- Phase 2 QA gate passed

### Phase 3 Success
- All 5 authorization tasks completed
- 100% of authorization logic covered by tests
- Security audit passed
- Phase 3 QA gate passed

### Phase 4 Success
- All 9 UI components completed and tested
- Mobile responsive
- No XSS vulnerabilities
- Phase 4 QA gate passed

### Phase 5 Success
- All tests passing (>85% coverage)
- Security audit passed
- Performance baseline met
- Build succeeds
- Documentation complete
- Ready for production deployment

### Overall Project Success
- Complete on schedule (18-26 days)
- Within team velocity
- Zero critical bugs
- 100% features implemented
- All acceptance criteria met
- Team confident in deployment

---

## File Structure Reference

```
Flycatcher/
├── Flycatcher.Models/
│   └── Database/
│       ├── Role.cs (NEW - Task 1.1)
│       ├── UserRole.cs (NEW - Task 1.2)
│       ├── UserReport.cs (NEW - Task 1.5)
│       ├── UserBan.cs (NEW - Task 1.6)
│       └── Message.cs (MODIFIED - Task 1.4)
├── Flycatcher.DataAccess/
│   ├── DataContext.cs (MODIFIED - Task 1.7)
│   └── QueryableRepository.cs (existing)
├── Flycatcher.Services/
│   ├── Enumerations/
│   │   ├── Permissions.cs (NEW - Task 1.3)
│   │   └── CallbackType.cs (MODIFIED - Task 2.9)
│   ├── RoleService.cs (NEW - Task 2.1)
│   ├── PermissionService.cs (NEW - Task 2.2)
│   ├── UserRoleService.cs (NEW - Task 2.3)
│   ├── MessageService.cs (MODIFIED - Task 2.4)
│   ├── UserReportService.cs (NEW - Task 2.5)
│   ├── UserBanService.cs (NEW - Task 2.6)
│   ├── MessageValidationService.cs (NEW - Task 2.7)
│   ├── UserService.cs (MODIFIED - Task 2.8)
│   ├── CallbackIdGenerator.cs (MODIFIED - Task 2.9)
│   ├── Attributes/
│   │   └── RequirePermissionAttribute.cs (NEW - Task 3.1)
│   ├── ChannelPermissionService.cs (NEW - Task 3.2)
│   ├── MessageAuthorizationService.cs (NEW - Task 3.3)
│   ├── SiteAdminService.cs (MODIFIED - Task 3.4)
│   └── UserTimeoutService.cs (NEW - Task 3.5, optional)
├── Flycatcher/
│   └── Components/
│       ├── Widgets/
│       │   ├── RoleAssignmentPopover.razor (NEW - Task 4.2)
│       │   ├── MessageDisplay.razor (MODIFIED - Task 4.3)
│       │   └── (existing components modified for role colors)
│       ├── Modals/
│       │   ├── RoleManagementDialog.razor (NEW - Task 4.1)
│       │   ├── CreateRoleDialog.razor (NEW - Task 4.1)
│       │   ├── EditRoleDialog.razor (NEW - Task 4.1)
│       │   ├── ReportUserDialog.razor (NEW - Task 4.4)
│       │   ├── BanUserDialog.razor (NEW - Task 4.6)
│       │   ├── ApproveAppealDialog.razor (NEW - Task 4.6)
│       │   └── SubmitBanAppealDialog.razor (NEW - Task 4.7)
│       └── Pages/
│           ├── AdminReportsDashboard.razor (NEW - Task 4.5)
│           └── AdminBansDashboard.razor (NEW - Task 4.6)
├── Flycatcher.Tests/
│   ├── RoleServiceTests.cs (NEW - Task 5.1)
│   ├── PermissionServiceTests.cs (NEW - Task 5.1)
│   ├── UserRoleServiceTests.cs (NEW - Task 5.1)
│   ├── MessageServiceTests.cs (NEW - Task 5.1)
│   ├── UserBanServiceTests.cs (NEW - Task 5.1)
│   ├── UserReportServiceTests.cs (NEW - Task 5.1)
│   ├── MessageAuthorizationServiceTests.cs (NEW - Task 5.1)
│   ├── RoleWorkflowIntegrationTests.cs (NEW - Task 5.2)
│   ├── MessageDeleteWorkflowTests.cs (NEW - Task 5.2)
│   ├── BanAppealWorkflowTests.cs (NEW - Task 5.2)
│   ├── PermissionEnforcementTests.cs (NEW - Task 5.2)
│   ├── RoleManagementComponentTests.cs (NEW - Task 5.3)
│   └── (other component tests)
├── IMPLEMENTATION_PLAN.md (NEW - Task 5.7)
├── FEATURES_ROLES_PERMISSIONS.md (NEW - Task 5.7)
├── FEATURES_MESSAGE_MODERATION.md (NEW - Task 5.7)
├── RELEASE_NOTES.md (NEW - Task 5.10)
└── CHANGELOG.md (MODIFIED - Task 5.10)
```

---

## Estimated Timeline

| Phase | Duration | Team Size | Key Milestones |
|-------|----------|-----------|-----------------|
| 1 | 3-5 days | 2-3 | Database schema complete, migration tested |
| 2 | 5-7 days | 2-3 | All services implemented, unit tests >80% |
| 3 | 3-4 days | 1-2 | Authorization layer complete, security review |
| 4 | 6-8 days | 2-3 | All UI components functional, responsive design |
| 5 | 4-6 days | 3-4 | All tests passing, security audit passed, ready to deploy |
| **Total** | **18-26 days** | **3-4 avg** | **Production ready** |

**Calendar View** (assuming start date Feb 24, 2026):
- Phase 1: Feb 24 - Feb 28 (Mon-Fri)
- Phase 2: Mar 1 - Mar 7 (Sun-Sat, with parallel Phase 4 starting Mar 1)
- Phase 3: Mar 8 - Mar 11 (Wed-Sat)
- Phase 4: Mar 1 - Mar 14 (Sun-Sat, parallel with Phase 2-3)
- Phase 5: Mar 15 - Mar 21 (Sun-Sat)

---

## Appendix: Detailed Acceptance Criteria

### Feature Set 1: Roles & Permissions (7 Features)

#### 1. Users can have up to 16 roles per server
- **Acceptance Criteria**:
  - User can be assigned 1-16 roles simultaneously
  - Cannot assign 17th role (error message shown)
  - Role list displays all assigned roles
  - UI shows "X/16 roles assigned"
  - Database enforces limit with check constraint

#### 2. Roles created/modified by users with "manage roles" permission
- **Acceptance Criteria**:
  - Only users with ManageRoles permission can create roles
  - Only users with ManageRoles permission can edit roles
  - Only server owner or admins have ManageRoles by default
  - Non-authorized users see permission denied message
  - Role creation validates name (1-32 chars)
  - Color picker works, stores hex value

#### 3. Roles assigned by users with "assign roles" permission
- **Acceptance Criteria**:
  - Only users with AssignRoles permission can assign roles
  - Can assign multiple roles to same user
  - Cannot exceed 16 roles per user
  - Role assignment shows success/error message
  - Reassigning same role prevents duplicate
  - UI updates in real-time via CallbackService

#### 4. Permissions: Send Messages, Delete others' messages, Timeout user, Ban user, Edit channels, Add channels, Edit server settings
- **Acceptance Criteria**:
  - All 7+ permissions available in role creation
  - Permissions displayed as checkboxes
  - Server owner has all permissions by default
  - Regular users inherit permissions from roles
  - Permissions combine via bitwise OR (multiple roles)
  - Permission checks prevent unauthorized actions
  - Database stores permissions as 64-bit bitmask

#### 5. UI for creating/editing roles
- **Acceptance Criteria**:
  - Role creation dialog appears on button click
  - Form validates all inputs before submission
  - Color picker shows and saves color
  - Permission checkboxes allow selection
  - Edit dialog shows current values
  - Delete confirmation prevents accidental deletion
  - Responsive design on mobile (dialog scrollable)

#### 6. Context menu on users in right sidebar to manage roles
- **Acceptance Criteria**:
  - Right-click (or context menu button) on user shows menu
  - "Manage Roles" option appears if authorized
  - Dialog shows available roles with checkboxes
  - Shows "X/16" counter
  - Apply/Cancel buttons work
  - UI updates immediately after apply
  - Non-authorized users don't see option

#### 7. Channel-level role visibility and talk permissions (with role color for usernames)
- **Acceptance Criteria**:
  - Roles have optional color (hex string)
  - User's primary role color displays next to name in messages
  - Role name shows on hover (tooltip)
  - Message author shows colored name (by highest role)
  - User list shows role colors
  - Channel-specific overrides can be configured (future)
  - Fallback to default color if no role color

### Feature Set 2: Message & Moderation (6 Features)

#### 8. Delete own messages (soft delete)
- **Acceptance Criteria**:
  - Users can delete their own messages
  - Delete button visible only on own messages
  - Message shows "[Message deleted]" after deletion
  - Original content not visible (except to admins)
  - Deleted message timestamp and who deleted it shown
  - Soft delete, not hard delete (recoverable)
  - Database tracks DeletedByUserId and DeletedAtUtc
  - No hard delete option for regular users

#### 9. Report a user
- **Acceptance Criteria**:
  - Users can report other users
  - Context menu on user shows "Report User" option
  - Report dialog requires reason (10-500 chars)
  - Cannot report yourself (error message)
  - Prevent duplicate reports same user within 24h
  - Success message: "User reported. Thank you for helping keep our community safe."
  - Admin notified of new report
  - Database tracks report details

#### 10. Reports UI for site admins
- **Acceptance Criteria**:
  - Admin-only dashboard shows all reports
  - Data grid with: user, reporter, reason, date, status
  - Filter by status (pending/reviewed/dismissed)
  - Sort by date, reported user
  - View full report details
  - Dismiss report button
  - Ban user button (with additional reason)
  - Statistics: pending count, reviewed count
  - Only site admins can access page

#### 11. Site admin permaban
- **Acceptance Criteria**:
  - Site admins can ban users globally
  - Ban dialog: username, reason (required)
  - Ban reason shown to admins and user
  - Banned users cannot login
  - Banned users cannot access any servers
  - Cannot ban site admins
  - Unban option for admins (emergency)
  - Database tracks who banned, when, why

#### 12. Permabanned user appeal (one appeal, approve/deny by site admins)
- **Acceptance Criteria**:
  - Banned users see appeal form on login
  - Appeal form: reason textarea (20-500 chars)
  - Submit appeal button
  - Only one appeal allowed per ban
  - Subsequent attempts show "appeal already submitted"
  - Admins see pending appeals list
  - Admin can approve (unban user) or deny
  - Approve unsets ban, user can login
  - Deny shows reason to user
  - Database tracks appeal status and review

#### 13. UTC timestamps stored, displayed in user local time (JS interop)
- **Acceptance Criteria**:
  - All timestamps stored as UTC in database
  - Timestamps converted to user's local time on display
  - Uses JavaScript interop for timezone detection
  - Format: "Feb 20, 2026 3:45 PM" or similar
  - Timezone displayed or inferrable
  - Works across DST transitions
  - Fallback to UTC if JS unavailable
  - Admin dashboard shows UTC timestamp as well

#### 14. 4000 character message limit
- **Acceptance Criteria**:
  - Message compose box prevents >4000 chars
  - Character counter shows "X/4000"
  - Send button disabled if empty or >4000
  - Warning at 80% (3200 chars) - yellow
  - Error state at 100% (4000 chars) - red
  - Error message if user tries to paste >4000
  - Backend enforces limit (validation)
  - Prevents Unicode exploit (count chars, not bytes)
  - Existing messages not modified

---

## Conclusion

This comprehensive implementation plan provides:

1. **Clear Structure**: 50 individual tasks organized into 5 logical phases
2. **Dependencies Mapped**: Each task lists what must be completed first
3. **Team Flexibility**: Tasks can be distributed across different team members in parallel
4. **Quality Gates**: Phase-end QA gates ensure quality before proceeding
5. **Risk Mitigation**: Identified risks with mitigation strategies
6. **Success Criteria**: Clear acceptance criteria for each feature
7. **Timeline**: 18-26 days with typical team of 3-4 developers
8. **Documentation**: Comprehensive documentation required at each phase

The plan balances speed with quality by allowing parallel work while maintaining strict quality gates. Each phase is designed to be independently testable before integration with subsequent phases.
