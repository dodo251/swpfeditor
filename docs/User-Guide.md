# SMS Script Editor - User Guide

## Overview
The SMS Script Editor is a WPF application for editing XML test scripts with advanced drag-and-drop capabilities, template system, and comprehensive undo/redo functionality.

## Interface Layout

### Main Window
- **Left Panel**: Template library with drag-and-drop templates
- **Center Panel**: XML script tree view with hierarchical display
- **Right Panel**: Property editor (top) and activity log (bottom)
- **Toolbar**: File operations, undo/redo, validation, and settings

## Key Features

### 🔄 Drag and Drop Operations

#### Moving Elements
1. Select any element in the script tree
2. Drag to a new valid location in the tree
3. Green highlight indicates valid drop zones
4. Red highlight indicates invalid drop zones
5. Release to complete the move operation

#### Using Templates
1. Select a template from the left panel
2. Drag into the script tree structure
3. Only valid parent-child combinations are allowed
4. Template instances are created with unique IDs

### 📝 Template System

#### Available Templates
- **Step Templates**: SSH, HTTP, Manual verification steps
- **Container Templates**: Steps, Sections, Sessions containers
- **Data Templates**: Extract and Check elements
- **Session Templates**: SSH and HTTP connection configurations

#### Template Features
- Automatic unique ID generation
- Parent-child validation based on XSD rules
- Default attribute values for common scenarios
- Deep copy creation prevents template corruption

### ↩️ Undo/Redo System

#### Supported Operations
- Attribute modifications
- Element addition and removal
- Element moving and reorganization
- Template application

#### Usage
- **Undo**: Ctrl+Z or toolbar button
- **Redo**: Ctrl+Y or toolbar button
- **History**: Up to 50 operations (configurable)
- **Automatic**: All operations tracked automatically

### 🎯 Context Menu Operations

Right-click on any tree element for:
- **Add Child Element**: Insert valid child elements
- **Delete Element**: Remove selected element
- **Copy/Paste**: Duplicate elements with validation
- **Expand/Collapse All**: Tree navigation helpers

### ⚙️ Settings Configuration

Access via toolbar "设置" button:

#### Validation Settings
- **Strong Validation Before Save**: Enable/disable comprehensive validation
- **Show Line Numbers**: Include line numbers in validation messages

#### UI Settings
- **Indent Width**: XML formatting indentation (1-10 spaces)
- **Auto-Expand Delay**: Hover delay for tree expansion (100-5000ms)
- **Auto-Load Sample**: Load sample file on startup

#### Undo/Redo Settings
- **Max Undo Steps**: Maximum operation history (10-1000)

#### Logging Settings
- **Max Log File Size**: File rotation threshold (1-100MB)

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+N | New file |
| Ctrl+O | Open file |
| Ctrl+S | Save file |
| Ctrl+Z | Undo |
| Ctrl+Y | Redo |
| F5 | Validate |

## Validation Rules

The editor enforces XSD-based parent-child relationships:

### Valid Hierarchies
```
test
├── meta, description, displayOrder
├── sessions
│   └── session*
├── steps
│   └── step*
│       ├── params, headers, interaction
│       └── extracts
│           └── extract*
│               └── checks
│                   └── check*
└── sections
    └── section*
        └── refs
            └── ref*
```

### Common Validation Errors
- **Invalid Parent-Child**: Elements in wrong containers
- **Missing Required Attributes**: ID, name, or type attributes
- **Duplicate IDs**: Conflicting identifier values
- **Invalid References**: Broken sourceRef or step references

## File Operations

### Supported Formats
- **Primary**: XML files with UTF-8 encoding
- **Templates**: Template.xml for custom templates
- **Settings**: settings.json for application preferences
- **Logs**: app.log for activity tracking

### File Locations
- **Application Data**: `%AppData%\SwpfEditor\`
- **Settings**: `%AppData%\SwpfEditor\settings.json`
- **Logs**: `%AppData%\SwpfEditor\app.log`

## Best Practices

### Template Usage
1. Start with container templates (steps, sections, sessions)
2. Add specific element templates within containers
3. Use SSH/HTTP step templates for common scenarios
4. Customize generated elements through property editor

### Organization
1. Use meaningful aliases for better readability
2. Group related steps in sections
3. Keep session configurations at the top level
4. Use consistent naming conventions

### Validation
1. Validate frequently during editing (F5)
2. Address validation errors before saving
3. Use strong validation for production files
4. Check activity log for detailed error information

## Troubleshooting

### Common Issues

#### Drag and Drop Not Working
- Ensure valid parent-child relationship
- Check if element is already in target location
- Verify target is not a descendant of source

#### Template Not Creating Elements
- Check template file format
- Verify parent-child validation rules
- Review activity log for error details

#### Undo/Redo Unavailable
- Check if maximum history reached
- Verify operation was tracked (attributes changes)
- Review settings for history configuration

#### Performance Issues
- Large files (>5000 nodes) may be slower
- Use expand/collapse to manage view complexity
- Check log file size and rotation settings

### Getting Help
1. Check activity log for error details
2. Review validation messages for specific issues
3. Verify file format against sample files
4. Check settings for correct configuration

## Advanced Features

### Custom Templates
1. Create Template.xml with custom definitions
2. Use standard template format with allowedParents
3. Include default attributes and element structure
4. Test with sample parent elements

### Batch Operations
1. Use copy/paste for duplicating similar elements
2. Create templates for common patterns
3. Use context menu for bulk expand/collapse
4. Leverage keyboard shortcuts for efficiency

### Performance Optimization
- Use lazy loading for large trees
- Minimize real-time validation
- Configure appropriate undo history limits
- Monitor log file sizes for rotation