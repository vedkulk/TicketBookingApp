# C# Basics + Docker/PostgreSQL Notes

## 1. Properties vs Fields

### Field

A **field** is a variable stored directly inside a class.

```csharp
public int price;
```

It directly stores data and can be accessed according to its access modifier.

### Property

A **property** provides controlled access to data.

```csharp
public int Price { get; set; }
```

Think of it as a controlled interface around a value.

* `get` → allows reading
* `set` → allows changing

The compiler generates the underlying storage/accessors for an auto-property.

### Why use properties?

Properties give you more control and are the standard way to expose data from classes.

```csharp
public int Price { get; set; }
```

Later, you can change the implementation:

```csharp
public int Price
{
    get { return price; }
    set
    {
        if (value >= 0)
            price = value;
    }
}
```

So:

> **Field = storage**
> **Property = controlled access to data**

---

## 2. Access Modifiers

Common access modifiers:

```text
public       → accessible from anywhere
private      → accessible only inside the class
protected    → class + derived classes
internal     → accessible within the same assembly/project
```

If you don't specify an access modifier for a **class member**, it is generally `private` by default.

```csharp
class Product
{
    int price;
}
```

is effectively:

```csharp
class Product
{
    private int price;
}
```

Therefore, if you want other classes to use something, explicitly make it `public`:

```csharp
public int Price { get; set; }
```

### Important distinction

A top-level class without a modifier is `internal` by default:

```csharp
class Product
{
}
```

So "default is private" mainly applies to **members inside a class**, not every C# declaration.

---

## 3. Naming Conventions

C# convention:

### PascalCase

Used for:

* Classes
* Methods
* Public properties
* Public members

```csharp
public class Product
{
    public int Price { get; set; }

    public void CalculatePrice()
    {
    }
}
```

### camelCase

Typically used for:

* Parameters
* Local variables
* Private fields

```csharp
public void SetPrice(int price)
{
    int discountedPrice = price - 10;
}
```

Common private-field convention:

```csharp
private int _price;
```

So:

```text
Class              → Product
Property           → Price
Method             → CalculatePrice()
Parameter          → price
Local variable     → discountedPrice
Private field      → _price
```

These are **conventions**, not compiler requirements.

---

## 4. Constructors

A constructor is a special method that runs when an object is created.

```csharp
public class Product
{
    public int Id { get; set; }
    public decimal Price { get; set; }

    public Product(int id, decimal price)
    {
        Id = id;
        Price = price;
    }
}
```

Creating the object:

```csharp
Product product = new Product(1, 100);
```

The constructor automatically runs.

### Why doesn't C# use a `constructor` keyword?

Because C# identifies a constructor by:

1. Having the **same name as the class**
2. Having **no return type**

```csharp
public Product(int id)
{
    Id = id;
}
```

Compare TypeScript:

```typescript
constructor(id: number) {
    this.id = id;
}
```

C# instead:

```csharp
public Product(int id)
{
    Id = id;
}
```

The class name itself tells C# that this is a constructor.

---

## 5. Parameter Name vs Property Name

You'll commonly see:

```csharp
public class Product
{
    public int Id { get; set; }

    public Product(int id)
    {
        Id = id;
    }
}
```

Why `id` and `Id`?

Because they represent two different things:

```text
id  → constructor parameter
Id  → object's property
```

This also avoids **shadowing**.

For example:

```csharp
public Product(int Id)
{
    Id = Id;
}
```

This is problematic because both names refer to the parameter within that scope. You haven't clearly referred to the object's property.

You could solve it using `this`:

```csharp
public Product(int Id)
{
    this.Id = Id;
}
```

But the conventional approach is cleaner:

```csharp
public Product(int id)
{
    Id = id;
}
```

### Mental model

```text
Id = id;
↑    ↑
property  parameter
```

The capitalization makes the distinction immediately obvious.

---

## 6. `{ get; }` vs `{ get; set; }`

### Mutable property

```csharp
public decimal Price { get; set; }
```

Can be read and changed:

```csharp
product.Price = 500;
```

### Read-only after construction

```csharp
public int Id { get; }
```

Can be assigned during construction:

```csharp
public Product(int id)
{
    Id = id;
}
```

But afterwards:

```csharp
product.Id = 10; // ❌
```

This is useful for values that shouldn't change once an object has been created.

### Example: Product

```csharp
public class Product
{
    public int Id { get; }
    public decimal Price { get; set; }

    public Product(int id, decimal price)
    {
        Id = id;
        Price = price;
    }
}
```

Reasoning:

```text
Id
→ identifies the product
→ shouldn't normally change
→ get-only

Price
→ can change
→ get + set
```

This is an important principle:

> **Make data as immutable as possible unless it genuinely needs to change.**

---

# Docker + PostgreSQL

## 7. `-e` — Environment Variables

When running PostgreSQL:

```bash
docker run \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=password \
  -e POSTGRES_DB=mydb \
  postgres
```

`-e` means:

> Set an environment variable inside the container.

The PostgreSQL Docker image specifically looks for variables such as:

```text
POSTGRES_USER
POSTGRES_PASSWORD
POSTGRES_DB
```

These tell the image how to initialize PostgreSQL.

### Important

These aren't generic Docker requirements.

Docker doesn't inherently know what `POSTGRES_USER` means.

The **PostgreSQL image's startup logic** understands these variables.

---

## 8. `-p host:container`

Example:

```bash
-p 5432:5432
```

Means:

```text
HOST PORT : CONTAINER PORT
     5432 : 5432
```

The PostgreSQL server is listening on port `5432` **inside the container**.

Docker exposes it through port `5432` on your machine.

You could instead do:

```bash
-p 5000:5432
```

Now:

```text
Your computer
localhost:5000
      ↓
Docker
container:5432
      ↓
PostgreSQL
```

The numbers don't have to match.

### Why two numbers?

Because they're two different networking environments:

```text
HOST                  CONTAINER
5000       →          5432
```

Your application connects to:

```text
localhost:5000
```

while PostgreSQL itself continues listening on:

```text
5432
```

inside the container.

---

# 9. `-v` — Volumes

Example:

```bash
docker run \
  -v postgres-data:/var/lib/postgresql/data \
  postgres
```

This creates/mounts a **named volume**.

```text
postgres-data
      ↓
/var/lib/postgresql/data
      ↓
PostgreSQL data
```

The important idea:

> **Containers are disposable; volumes are persistent.**

If you remove the container:

```bash
docker rm postgres
```

the container disappears.

But:

```text
postgres-data
```

still exists.

When you create another PostgreSQL container and mount the same volume:

```bash
-v postgres-data:/var/lib/postgresql/data
```

PostgreSQL can access the old database files.

---

# 10. Proof of Persistence

The experiment you performed is the best way to understand Docker volumes.

### Step 1 — Create PostgreSQL container

```bash
docker run --name postgres \
  -e POSTGRES_PASSWORD=password \
  -e POSTGRES_DB=testdb \
  -v postgres-data:/var/lib/postgresql/data \
  -p 5432:5432 \
  -d postgres
```

### Step 2 — Create a table

Connect to PostgreSQL and create something:

```sql
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name TEXT
);
```

Insert data:

```sql
INSERT INTO products (name)
VALUES ('Laptop');
```

Check:

```sql
SELECT * FROM products;
```

You should see the data.

### Step 3 — Destroy the container

```bash
docker rm -f postgres
```

The **container** is gone.

But the volume remains.

### Step 4 — Create a new container

Create another PostgreSQL container using the same volume:

```bash
-v postgres-data:/var/lib/postgresql/data
```

### Step 5 — Check the table

```sql
SELECT * FROM products;
```

The table and data are still there.

### What did we prove?

```text
Container destroyed
       ↓
Container's writable layer gone
       ↓
Volume survives
       ↓
New container mounts same volume
       ↓
PostgreSQL sees old data
```

That's the fundamental purpose of persistent volumes.

---

# 11. `docker exec -it ... psql`

Example:

```bash
docker exec -it postgres psql -U postgres -d testdb
```

Break it down:

```text
docker exec
```

Run a command **inside an already-running container**.

```text
-it
```

Interactive terminal.

* `-i` → keep input open
* `-t` → allocate a terminal

```text
postgres
```

The container name.

```text
psql
```

PostgreSQL's command-line client.

```text
-U postgres
```

Connect as PostgreSQL user `postgres`.

```text
-d testdb
```

Connect to database `testdb`.

---

## Why does this bypass the port?

Normally:

```text
Your computer
     ↓
localhost:5432
     ↓
Docker port mapping
     ↓
PostgreSQL container
```

With:

```bash
docker exec -it postgres psql ...
```

you instead do:

```text
Docker container
      ↓
psql
      ↓
PostgreSQL
```

You're executing `psql` **inside the container itself**.

Therefore, this is a useful way to verify that PostgreSQL and its data are working **without involving host-to-container port mapping**.

---

# Key Mental Models

### C#

```text
Field
  ↓
actual stored variable

Property
  ↓
controlled interface to data

get
  ↓
read

set
  ↓
modify

get only
  ↓
read-only after initialization
```

### Docker

```text
Image
  ↓
Template for creating containers

Container
  ↓
Running instance
  ↓
Disposable

Volume
  ↓
Persistent data
  ↓
Survives container deletion
```

### PostgreSQL + Docker

```text
-e
↓
Configure container through environment variables

-p
↓
Connect host port → container port

-v
↓
Persist database data

docker exec
↓
Run a command directly inside the container
```

**The biggest concept to remember:** Docker doesn't make your database persistent by itself. **The volume is what separates your database's lifetime from the container's lifetime.**