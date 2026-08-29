\# Shift Dynamics - API Contract



\## 1. API Overview



The Shift Dynamics frontend communicates with the ASP.NET Core backend

through REST APIs.



All API responses should use JSON.



\## 2. Authentication



\### Login



POST `/api/auth/login`



\### Register



POST `/api/auth/register`



\### Current User



GET `/api/auth/me`



\## 3. Customers



GET `/api/customers`



GET `/api/customers/{id}`



POST `/api/customers`



PUT `/api/customers/{id}`



DELETE `/api/customers/{id}`



\## 4. Vehicles



GET `/api/vehicles`



GET `/api/vehicles/{id}`



POST `/api/vehicles`



PUT `/api/vehicles/{id}`



DELETE `/api/vehicles/{id}`



\## 5. Appointments



GET `/api/appointments`



GET `/api/appointments/{id}`



POST `/api/appointments`



PUT `/api/appointments/{id}`



DELETE `/api/appointments/{id}`



\## 6. Work Orders



GET `/api/work-orders`



GET `/api/work-orders/{id}`



POST `/api/work-orders`



PUT `/api/work-orders/{id}`



DELETE `/api/work-orders/{id}`



\## 7. Services



GET `/api/services`



GET `/api/services/{id}`



POST `/api/services`



PUT `/api/services/{id}`



DELETE `/api/services/{id}`



\## 8. Inventory



GET `/api/inventory`



GET `/api/inventory/{id}`



POST `/api/inventory`



PUT `/api/inventory/{id}`



DELETE `/api/inventory/{id}`



\## 9. Employees



GET `/api/employees`



GET `/api/employees/{id}`



POST `/api/employees`



PUT `/api/employees/{id}`



DELETE `/api/employees/{id}`



\## 10. Invoices



GET `/api/invoices`



GET `/api/invoices/{id}`



POST `/api/invoices`



PUT `/api/invoices/{id}`



\## 11. Payments



GET `/api/payments`



GET `/api/payments/{id}`



POST `/api/payments`



\## 12. Standard HTTP Status Codes



200 - Successful request



201 - Resource created



204 - Successful request with no response body



400 - Invalid request



401 - Authentication required



403 - Insufficient permissions



404 - Resource not found



409 - Conflict



500 - Internal server error



\## 13. API Rules



\- Requests and responses use JSON.

\- Endpoints use plural resource names where appropriate.

\- IDs are used to identify individual resources.

\- Authentication is required for protected endpoints.

\- Validation errors should return an appropriate 400 response.

\- Unauthorized requests should return 401.

\- Requests from authenticated users without sufficient permissions

&#x20; should return 403.

