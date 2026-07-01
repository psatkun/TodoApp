# TodoApp Application

This repository contains the backend RESTful API and the frontend dashboard (built in React/Vite) for a full-stack, secure task management application built using ASP.NET Core and Entity Framework Core. 

The architecture is explicitly designed around isolated data multi-tenancy, ensuring rigid ownership boundaries where users can only interact with tasks they created.

---

## How to Start the Application

### Prerequisites
* .NET 9.0 SDK
* Node.js (v18.0.0 or higher recommended) & npm (for running the frontend submodule)
* A command-line terminal (Bash, Zsh, or PowerShell)

### Installation & Execution Steps

#### 1. Clone the Repository & Fetch Submodules
Because the frontend lives as a Git submodule, you need to initialize it when cloning.

**If cloning for the first time:**
```bash
git clone --recursive <repository-url>
```

**If you already cloned the repository normally (and the `todo-frontend` folder is empty):**
```bash
git submodule init
git submodule update
```

#### 2. Run the Backend API
Restore NuGet dependencies and start the ASP.NET Core engine (the `TodoApp` profile ensures HTTPS):
```bash
cd ./TodoApp/TodoApp
dotnet restore
dotnet run --launch-profile TodoApp
```
The API will initialize, spin up an auto-migrating local SQLite database instance, and map the web server locally:
* **Base URL:** `https://localhost:7128`
* **Swagger Documentation:** `https://localhost:7128/swagger`

#### 3. Run the Frontend (Submodule)
In a new terminal window, navigate to the frontend directory to install dependencies and spin up the client:
```bash
cd ./TodoApp/todo-frontend
npm install
npm start # or 'npm run dev' depending on your frontend framework
```

### Running the Unit Test Suite
The testing matrix uses xUnit and an isolated EF Core In-Memory database engine to thoroughly test logic validations, identity collisions, and multi-tenant isolation rules without data bleed.

To execute the test project cleanly without entry-point compilation conflicts:

```dotnet test -p:BuildingForTest=true```

---

### Architectural Style & Considerations

The backend adheres to a lightweight Layered Architecture pattern, balancing decoupling with rapid execution:

* Controller / presentation layer: Manages incoming HTTP pipelines, processes model routing validations, and formats standardized outward responses.
* Data access layer (EF Core): Isolates raw query generation, utilizing DbContext abstraction over the relational database store.
* Security boundary placement: Authentication and token state verification are handled globally via native ASP.NET Core Middleware. Resource authorization is verified at the controller boundary, checking incoming model data against security claims extracted from the execution context.
* Performance and footprint: Synchronous I/O blocks are eliminated. All database round-trips and framework evaluation sequences run asynchronously (`async/await`) to maximize throughput under heavy parallel request loads.

---

## What Was Built

The application implements a full-stack task-tracking system backed by a relational database, utilizing clean controller patterns and data transfer objects (DTOs) to keep internal entities hidden from the HTTP payload space.

* Authentication - AuthController: core registration and secure login pipelines. Passwords are encrypted utilizing strong cryptographic hashing schemes before persistence.
* Task lifecycle manager - TodosController: A secure REST endpoint set handling complete CRUD operations for task items.
* Multitenant Data Enforcements: All task-level queries run through a contextual validation pipeline. The runtime reads the user identifier directly from the authenticated token claims, ensuring an authenticated user cannot intercept, update, or enumerate records belonging to another database tenant.
* Custom model validation guards: 
  * Blanks, empty titles, or strings consisting entirely of whitespace are caught at the entry barrier.
  * Due dates are filtered through a custom native attribute ([FutureDate]) ensuring historical dates are rejected cleanly before data state modifications.

---

## What Was Deliberately Left Out (And Why)

* Refresh tokens: The app uses relatively short-lived JWT tokens to accomplish security, but sessions expire quickly. I left out a refresh token system to keep authentication logic simple and focused, but a more scalable production
should allow users to be logged in for longer time periods.
* Database index optimization -- The current simple implementation of the application does not really need any optimized complex indexing and may slow the application down.
* Advanced logging -- The only logging in the application is to the console, but there are no advanced third-party logging for purposes such as crash reporting or monitoring (with services like Raygun, etc). This 
* may be useful as the application scales.

---

## What I Would Do with Another Day

If granted an extra day of development cycles, the transition to production-ready status would prioritize the following:

* Global exception handling: I would have Implemented a more unified, custom middleware exception layer to catch unexpected app panics. This would ensure that the API always returns a JSON payload response to the frontend client, neutralizing the danger of accidental stack-trace exposure.
* Improved logging: I would have added more logging for debugging / troubleshooting purposes
* More features related to task items -- priority, owner (other than the creator of the task)
* Cleaner, more minimalistic UI with unified color palettes
* Stricter requirements for username / password creation (e.g. at least 1 upper case, special characters, minimum length)

## Future Considerations for Scalability

* Database caching -- A caching layer like Redis would be useful to store frequently accessed data in memory, which would reduce the load on the database and reduce response times.
* Production database -- SQLite has a single file adtabase, but a production environment with concurrent users would necessitate a more robust relational database like PostgreSQL or SQL Server that could handle thousands of simultaneous reads and writes
* Pagination -- In the current implementation, fetching tasks returns the entire list of a user. If this list was on the order of hundreds, this would greatly slow down the fetch. Pagination (such as returning 20-25 items at a time) would ensure that payloads are lighter and more fastly transmitted.
* Horizontal scaling -- Multiple instances of the application could exist behind a load balancer. If traffic spiked, we could spin up more API containers to distribute work more evenly.

