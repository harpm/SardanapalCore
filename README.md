# Sardanapal Core - A Reliable Framework for Fast Business Solutions

## Overview

Welcome to **Sardanapal Core**, a comprehensive framework designed to empower businesses with reliable and efficient software solutions. Our goal is to streamline the development process, enabling you to launch your projects quickly and effectively. With a unique request-response protocol, reusable static functions, and a strong service abstraction layer, Sardanapal Core is your go-to solution for modern business applications.

## Features

- **Fast Start-Up**: Quickly configure your projects with minimal setup time.
  
- **Unique Request-Response Protocol**: Efficiently handle communications between services with our optimized protocol.

- **Reusable Static Functions**: Access a library of static methods that simplify common tasks across different modules.

- **Service Abstraction**: Interact seamlessly with external services while maintaining clean and maintainable code.

## Unique Request-Response Protocol

Our innovative request-response protocol is designed for speed and reliability. It utilizes a lightweight structure that minimizes overhead while ensuring robust communication between components. Key aspects include:

- **Two Datagram Communication**: Reduces the number of messages exchanged, enhancing performance.

- **Resilience**: Built-in mechanisms to handle message loss and delays, ensuring consistent application behavior.

### Example Protocol Structure
```
interface IResponse<TData>
{
  TData Data;
  byte statusCode;
  byte operationType;
  string[] developerMessages;
  string userMessages;
}
```
## Reusable Static Functions

**Sardanapal Core** includes a set of static functions that can be reused across your projects, improving efficiency and reducing redundancy. Examples include:

- **Data Validation**: Simplify input validation processes.
- **Service Management**: Centralize service-related operations.
 
## Service Abstraction

Our service abstraction layer allows you to easily integrate with various APIs without tightly coupling your code to specific implementations. This enhances maintainability and scalability.
### CrudServiceBase and ServicePanelBase are the best examples
## Installation
To get started with **Sardanapal Core**, follow these steps:
1. Ask for the public token (arsalan.hadidi1379@gmail.com)
2. Add the nuget profile using the package and url Given in the email response
3. install the provided nuget packages accordingly

### Thanks for your attention
