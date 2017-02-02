# Introduction

Dapper Extensions is a small library that complements [Dapper](https://github.com/SamSaffron/dapper-dot-net) by adding basic CRUD operations (Get, Insert, Update, Delete) for your POCOs. For more advanced querying scenarios, Dapper Extensions provides a predicate system. The goal of this library is to keep your POCOs pure by not requiring any attributes or base class inheritance.

Customized mappings are achieved through [ClassMapper](https://github.com/tmsmith/Dapper-Extensions/wiki/AutoClassMapper). 

**Important**: This library is a separate effort from Dapper.Contrib (a sub-system of the [Dapper](https://github.com/SamSaffron/dapper-dot-net) project).

Features
--------
* Zero configuration out of the box.
* Automatic mapping of POCOs for Get, Insert, Update, and Delete operations.
* GetList, Count methods for more advanced scenarios.
* GetPage for returning paged result sets.
* Automatic support for Guid and Integer [primary keys](https://github.com/tmsmith/Dapper-Extensions/wiki/KeyTypes) (Includes manual support for other key types).
* Pure POCOs through use of [ClassMapper](https://github.com/tmsmith/Dapper-Extensions/wiki/AutoClassMapper) (_Attribute Free!_).
* Customized entity-table mapping through the use of [ClassMapper](https://github.com/tmsmith/Dapper-Extensions/wiki/AutoClassMapper).
* Composite Primary Key support.
* Singular and Pluralized table name support (Singular by default).
* Easy-to-use [Predicate System](https://github.com/tmsmith/Dapper-Extensions/wiki/Predicates) for more advanced scenarios.
* Properly escapes table/column names in generated SQL (Ex: SELECT [FirstName] FROM [Users] WHERE [Users].[UserId] = @UserId_0)
* Unit test coverage (150+ Unit Tests)

Naming Conventions
------------------
* POCO names should match the table name in the database. Pluralized table names are supported through the PlurizedAutoClassMapper.
* POCO property names should match each column name in the table.
* By convention, the primary key should be named Id. Using another name is supported through custom mappings.

# Installation

For more information, please view our [Getting Started](https://github.com/tmsmith/Dapper-Extensions/wiki/Getting-Started) guide.

http://nuget.org/List/Packages/DapperExtensions

```
PM> Install-Package DapperExtensions
```

# Examples
The following examples will use a Person POCO defined as:

```c#
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool Active { get; set; }
    public DateTime DateCreated { get; set; }
}
```


## Get Operation

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    int personId = 1;
    Person person = cn.Get<Person>(personId);	
    cn.Close();
}
```

## Simple Insert Operation

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Person person = new Person { FirstName = "Foo", LastName = "Bar" };
    int id = cn.Insert(person);
    cn.Close();
}
```

## Advanced Insert Operation (Composite Key)

```c#
public class Car
{
    public int ModelId { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
}

...

using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Car car = new Car { Color = "Red" };
    var multiKey = cn.Insert(car);
    cn.Close();

    int modelId = multiKey.ModelId;
    int year = multiKey.Year;
}
```

## Simple Update Operation

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    int personId = 1;
    Person person = cn.Get<Person>(personId);
    person.LastName = "Baz";
    cn.Update(person);
    cn.Close();
}
```


## Simple Delete Operation

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    Person person = cn.Get<Person>(1);
    cn.Delete(person);
    cn.Close();
}
```

## GetList Operation (with Predicates)

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    var predicate = Predicates.Field<Person>(f => f.Active, Operator.Eq, true);
    IEnumerable<Person> list = cn.GetList<Person>(predicate);
    cn.Close();
}
```

Generated SQL

```
SELECT 
   [Person].[Id]
 , [Person].[FirstName]
 , [Person].[LastName]
 , [Person].[Active]
 , [Person].[DateCreated] 
FROM [Person] 
WHERE ([Person].[Active] = @Active_0)
```

More information on predicates can be found in [our wiki](https://github.com/tmsmith/Dapper-Extensions/wiki/Predicates).


## Count Operation (with Predicates)

```c#
using (SqlConnection cn = new SqlConnection(_connectionString))
{
    cn.Open();
    var predicate = Predicates.Field<Person>(f => f.DateCreated, Operator.Lt, DateTime.UtcNow.AddDays(-5));
    int count = cn.Count<Person>(predicate);
    cn.Close();
}            
```

Generated SQL

```
SELECT 
   COUNT(*) Total 
FROM [Person] 
WHERE ([Person].[DateCreated] < @DateCreated_0)
```

More information on predicates can be found in [our wiki](https://github.com/tmsmith/Dapper-Extensions/wiki/Predicates).


# License

Copyright 2011 Thad Smith, Page Brooks and contributors

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
