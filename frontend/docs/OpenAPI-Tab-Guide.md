# OpenAPI Documents Tab - User Guide

## Overview
The new OpenAPI tab in the Prock interface allows you to upload, manage, and view OpenAPI specifications for your APIs. This feature enables you to store API documentation alongside your mock configurations.

## Features

### 📋 **Document Management**
- **Upload** OpenAPI specifications (JSON/YAML)
- **View** document details and metadata  
- **Activate/Deactivate** documents
- **Delete** documents (permanent delete)

### 📁 **File Upload Options**
1. **File Upload**: Select JSON/YAML files from your computer
2. **Manual Entry**: Paste OpenAPI content directly
3. **Auto-extraction**: Automatically extracts title, version, and description from uploaded files

### 🔍 **Document Viewing**
- **List View**: Shows all documents with status, version, and creation date
- **Detail Modal**: View comprehensive document information including:
  - Title, version, description
  - OpenAPI version
  - Host and base path information
  - Supported schemes (HTTP/HTTPS)
  - Creation and update timestamps

## How to Use

### 1. Access the OpenAPI Tab
Click on the "OpenAPI" tab in the main navigation to access the document management interface.

### 2. Upload a New Document
1. Click **"Upload New Document"** button
2. Fill in the form:
   - **Title**: Optional title override
   - **Version**: Optional version override  
   - **Description**: Optional description override
   - **Upload File**: Select your OpenAPI JSON/YAML file OR
   - **Manual Entry**: Paste your OpenAPI specification directly
3. Click **"Upload Document"** to save

### 3. View Documents
- The main interface shows a table of all active documents
- Click **"View"** button to see detailed document information
- Documents show status badges (Active/Inactive)

### 4. Manage Documents
- **Activate/Deactivate**: Toggle document status using the activate/deactivate button
- **Delete**: Remove documents permanently (with confirmation)

## File Formats Supported
- **JSON**: Standard OpenAPI 3.0+ JSON format
- **YAML**: OpenAPI YAML specifications (converted to JSON for storage)

## Integration with Prock
The OpenAPI documents are stored in your MongoDB database and can be:
- Queried via the REST API endpoints
- Used for generating mock responses
- Referenced for API documentation
- Integrated with other Prock features

## API Endpoints
The tab uses these backend endpoints:
- `GET /prock/api/openapi-documents` - List all documents
- `POST /prock/api/openapi-documents` - Create new document
- `GET /prock/api/openapi-documents/{id}` - Get document details
- `PUT /prock/api/openapi-documents/{id}` - Update document
- `DELETE /prock/api/openapi-documents/{id}` - Delete document

## Tips
- Documents are automatically validated when uploaded
- The interface will extract metadata from your OpenAPI files automatically
- Use the activate/deactivate feature to temporarily disable documents
- Documents are permanently deleted when removed; this action cannot be undone.

Enjoy managing your API specifications with Prock! 🚀
