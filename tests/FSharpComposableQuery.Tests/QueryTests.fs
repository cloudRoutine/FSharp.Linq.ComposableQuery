﻿namespace FSharpComposableQuery.Tests

open System
open System.Data.Linq.SqlClient
open System.Linq
open Microsoft.FSharp.Data.TypeProviders
open Microsoft.FSharp.Linq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open NUnit.Framework
open FSharp.Data.Sql

open FSharpComposableQuery

module QueryTests = 

    let [<Literal>] connectionString = "DataSource=" + __SOURCE_DIRECTORY__ + @"/../databases/simple.db;" + "Version=3;foreign keys = true"
    let [<Literal>] resolutionPath = __SOURCE_DIRECTORY__ + @"../../packages/test/System.Data.Sqlite.Core/net46"
    type sql = SqlDataProvider<
                Common.DatabaseProviderTypes.SQLITE
            ,   SQLiteLibrary = Common.SQLiteLibrary.SystemDataSQLite
            ,   ConnectionString = connectionString
            ,   ResolutionPath = resolutionPath
            ,   CaseSensitivityChange = Common.CaseSensitivityChange.ORIGINAL
            >

    [<TestFixture>]
    type TestClass() = 

        let db = sql.GetDataContext().Main

        let data = [1; 5; 7; 11; 18; 21]

        
        let mutable idx = 0
        // Generates a unique tag for each consecutive query
        let tag s = 
            idx <- idx + 1
            printfn "Q%02d: %s" idx s
        
        let printNullable (v:Nullable<'T>) =
            if (v.HasValue) then v.Value.ToString()
            else "NULL"


        [<Test>]
        member this.``contains query operator``() = 
            tag "contains query operator"
            let result1 =
                query {
                    for student in db.Student do
                    select student.Age
                    contains 11
                    }
            result1 |> printfn "Is at least one student age 11? %b" 


        [<Test>]
        member this.``count query operator``() = 
            tag "count query operator"
            let result2 =
                query {
                    for student in db.Student do
                    select student
                    count
                    }

            result2 |> printfn "Number of students: %d" 


        [<Test>]
        member this.``last query operator.``() = 
            tag "last query operator." 
            let result3 =
                query {
                    for s in data do
                    sortBy s
                    last
                    }
            printfn "Last: %d" result3


        [<Test>]
        member this.``lastOrDefault query operator.``() = 
            tag "lastOrDefault query operator." 
            let result4 =
                query {
                        for number in data do
                        sortBy number
                        lastOrDefault
                        }
            result4 |> printfn "lastOrDefault: %d"


        [<Test>]
        member this.``exactlyOne query operator.``() = 
            tag "exactlyOne query operator."
            let student2 =
                query {
                    for student in db.Student do
                    where (student.StudentId = 1)
                    select student
                    exactlyOne
                    }
            printfn "Student with StudentId = 1 is %s" student2.Name


        [<Test>]
        member this.``exactlyOneOrDefault query operator.``() = 
            tag "exactlyOneOrDefault query operator."
            let student3 =
                query {
                    for student in db.Student do
                    where (student.StudentId = 1)
                    select student
                    exactlyOneOrDefault
                    }
            printfn "Student with StudentId = 1 is %s" student3.Name


        [<Test>]
        member this.``headOrDefault query operator.``() = 
            tag "headOrDefault query operator."
            let student4 =
                query {
                    for student in db.Student do
                    select student
                    headOrDefault
                    }
            printfn "head student is %s" student4.Name


        [<Test>]
        member this.``select query operator.``() = 
            tag "select query operator."
            let select5 = 
                query {
                    for (student) in db.Student do
                    select student
                    }
            select5 |> Seq.iter (fun (student) -> printfn "StudentId, Name: %d %s" student.StudentId student.Name)


        [<Test>]
        member this.``where query operator.``() = 
            tag "where query operator."
            let select6 = 
                query {
                    for student in db.Student do
                    where (student.StudentId > 4)
                    select student
                    }
            select6 |> Seq.iter (fun student -> printfn "StudentId, Name: %d %s" student.StudentId student.Name)
            ignore 0

        [<Test>]
        member this.``minBy query operator.``() = 
            tag "minBy query operator."
            let student5 =
                query {
                    for student in db.Student do
                    minBy student.StudentId
                }
            ignore 0


        [<Test>]
        member this.``maxBy query operator.``() = 
            tag "maxBy query operator."
            let student6 =
                query {
                    for student in db.Student do
                    maxBy student.StudentId
                }
            ignore 0

    

        [<Test>]
        member this.``groupBy query operator.``() = 
            tag "groupBy query operator."
            query {
                for student in db.Student do
                groupBy student.Age into g
                select (g.Key, g.Count())
                }
            |> Seq.iter (fun (age, count) -> printfn "Age: %i Count at that age: %d"  age count)


        [<Test>]
        member this.``sortBy query operator.``() = 
            tag "sortBy query operator."
            query {
                for student in db.Student do
                sortBy student.Name
                select student
            }
            |> Seq.iter (fun student -> printfn "StudentId, Name: %d %s" student.StudentId student.Name)


        [<Test>]
        member this.``sortByDescending query operator.``() = 
            tag "sortByDescending query operator."
            query {
                for student in db.Student do
                sortByDescending student.Name
                select student
            }
            |> Seq.iter (fun student -> printfn "StudentId, Name: %d %s" student.StudentId student.Name)


        [<Test>]
        member this.``thenBy query operator.``() = 
            tag "thenBy query operator."
            query {
                for student in db.Student do
                where (student.Age >= 0)
                sortBy student.Age
                thenBy student.Name
                select student
            }
            |> Seq.iter (fun student -> printfn "StudentId, Name: %d %s" student.Age student.Name)


        [<Test>]
        member this.``thenByDescending query operator.``() = 
            tag "thenByDescending query operator."
            query {
                for student in db.Student do
                where (student.Age >= 0)
                sortBy student.Age
                thenByDescending student.Name
                select student
            }
            |> Seq.iter (fun student -> printfn "StudentId, Name: %d %s" student.Age student.Name)


        [<Test>]
        member this.``groupValBy query operator.``() = 
            tag "groupValBy query operator."
            query {
                for student in db.Student do
                groupValBy student.Name student.Age into g
                select (g, g.Key, g.Count())
                }
            |> Seq.iter (fun (group, age, count) ->
                printfn "Age: %i Count at that age: %i" age count
                group |> Seq.iter (fun name -> printfn "Name: %s" name))


        [<Test>]
        member this.``sumByNullable query operator``() = 
            tag "sumByNullable query operator"
            query {
                for student in db.Student do
                sumBy student.Age
                }
            |> (fun sum -> printfn "Sum of ages: %i" sum)


        [<Test>]
        member this.``minByNullable``() = 
            tag "minByNullable"
            query {
                for student in db.Student do
                minBy student.Age
                }
            |> (fun age -> printfn "Minimum age: %i" age)


        [<Test>]
        member this.``maxByNullable``() = 
            tag "maxByNullable"
            query {
                for student in db.Student do
                maxByNullable (Nullable student.Age)
                }
            |> (fun age -> printfn "Maximum age: %s" (printNullable age))


        [<Test>]
        member this.``averageBy``() = 
            tag "averageBy"
            query {
                for student in db.Student do
                averageBy (float student.StudentId)
                }
            |> printfn "Average student ID: %f"


        [<Test>]
        member this.``averageByNullable``() = 
            tag "averageByNullable"
            query {
                for student in db.Student do
                averageByNullable (Nullable (float student.Age))
                }
            |> (fun avg -> printfn "Average age: %s" (printNullable avg))


        [<Test>]
        member this.``find query operator``() = 
            tag "find query operator"
            query {
                for student in db.Student do
                find (student.Name = "Abercrombie, Kim")
            }
            |> (fun student -> printfn "Found a match with StudentId = %d" student.StudentId)


        [<Test>]
        member this.``all query operator``() = 
            tag "all query operator"
            query {
                for student in db.Student do
                all (SqlMethods.Like(student.Name, "%,%"))
            }
            |> printfn "Do all students have a comma in the name? %b"


        [<Test>]
        member this.``head query operator``() = 
            tag "head query operator"
            query {
                for student in db.Student do
                head
                }
            |> (fun student -> printfn "Found the head student with StudentId = %d" student.StudentId)


        [<Test>]
        member this.``nth query operator``() = 
            tag "nth query operator"
            query {
                for numbers in data do
                nth 3
                }
            |> printfn "Third number is %d"


        [<Test>]
        member this.``skip query operator``() = 
            tag "skip query operator"
            query {
                for student in db.Student do
                skip 1
                }
            |> Seq.iter (fun student -> printfn "StudentId = %d" student.StudentId)


        [<Test>]
        member this.``skipWhile query operator``() = 
            tag "skipWhile query operator"
            query {
                for number in data do
                skipWhile (number < 3)
                select number
                }
            |> Seq.iter (fun number -> printfn "Number = %d" number)


        [<Test>]
        member this.``sumBy query operator``() = 
            tag "sumBy query operator"
            query {
               for student in db.Student do
               sumBy student.StudentId
               }
            |> printfn "Sum of student IDs: %d" 


        [<Test>]
        member this.``take query operator``() = 
            tag "take query operator"
            query {
               for student in db.Student do
               select student
               take 2
               }
            |> Seq.iter (fun student -> printfn "StudentId = %d" student.StudentId)


        [<Test>]
        member this.``takeWhile query operator``() = 
            tag "takeWhile query operator"
            query {
                for number in data do
                takeWhile (number < 10)
                }
            |> Seq.iter (fun number -> printfn "Number = %d" number)


        [<Test>]
        member this.``sortByNullable query operator``() = 
            tag "sortByNullable query operator"
            query {
                for student in db.Student do
                sortByNullable (Nullable student.Age)
                select student
            }
            |> Seq.iter (fun student ->
                printfn "StudentId, Name, Age: %d %s %i" student.StudentId student.Name student.Age)


        [<Test>]
        member this.``sortByNullableDescending query operator``() = 
            tag "sortByNullableDescending query operator"
            query {
                for student in db.Student do
                sortByNullableDescending (Nullable student.Age)
                select student
            }
            |> Seq.iter (fun student ->
                printfn "StudentId, Name, Age: %d %s %i" student.StudentId student.Name student.Age)


        [<Test>]
        member this.``thenByNullable query operator``() = 
            tag "thenByNullable query operator"
            query {
                for student in db.Student do
                sortBy student.Name
                thenByNullable (Nullable student.Age)
                select student
            }
            |> Seq.iter (fun student ->
                printfn "StudentId, Name, Age: %d %s %i" student.StudentId student.Name student.Age)


        [<Test>]
        member this.``thenByNullableDescending query operator``() = 
            tag "thenByNullableDescending query operator"
            query {
                for student in db.Student do
                sortBy student.Name
                thenByNullableDescending (Nullable student.Age)
                select student
            }
            |> Seq.iter (fun student ->
                printfn "StudentId, Name, Age: %d %s %s" student.StudentId student.Name (printNullable <| Nullable student.Age))


        [<Test>]
        member this.``All students:``() = 
            tag "All students: "
            query {
                    for student in db.Student do
                    select student
                }
                |> Seq.iter (fun student -> printfn "%s %d %s" student.Name student.StudentId (printNullable <| Nullable student.Age))


        [<Test>]
        member this.``Count of students:``() = 
            tag "Count of students: "
            query {
                    for student in db.Student do        
                    count
                }
            |>  (fun count -> printfn "Student count: %d" count)




        (* This example is the same as above but works, because we use ExtraTopLevelOperators.query *)
        [<Test>]
        member this.``Exists.``() = 
            tag "Exists."
            query {
                    for student in db.Student do
                    where (ExtraTopLevelOperators.query 
                                  { for courseSelection in db.CourseSelection do
                                    exists (courseSelection.StudentId = student.StudentId) })
                    select student }
            |> Seq.iter (fun student -> printfn "%A" student.Name)
    

        (* This example demonstrates the bug *)
        [<Test>]
        member this.``Exists (bug).``() = 
            tag "Exists (bug)."
            query {
                    for student in db.Student do
                    where (query 
                                  { for courseSelection in db.CourseSelection do
                                    exists (courseSelection.StudentId = student.StudentId) })
                    select student }
            |> Seq.iter (fun student -> printfn "%A" student.Name)

        [<Test>]
        member this.``Group by age and count``() = 
            tag "Group by age and count"
            query {
                    for n in db.Student do
                    groupBy n.Age into g
                    select (g.Key, g.Count())
            }
            |> Seq.iter (fun (age, count) -> printfn "%s %d" (printNullable <| Nullable age) count)


        [<Test>]
        member this.``Group value by age.``() = 
            tag "Group value by age."
            query {
                    for n in db.Student do
                    groupValBy n.Age n.Age into g
                    select (g.Key, g.Count())
                }
            |> Seq.iter (fun (age, count) -> printfn "%s %d" (printNullable <| Nullable age) count)


    



        [<Test>]
        member this.``Group students by age where age > 10.``() = 
            tag "Group students by age where age > 10."
            query {
                    for student in db.Student do
                    groupBy student.Age into g
                    where ( g.Key > 10)
                    select (g, g.Key)
            }
            |> Seq.iter (fun (students, age) ->
                printfn "Age: %s" (age.ToString())
                students
                |> Seq.iter (fun student -> printfn "%s" student.Name))

        [<Test>]
        member this.``Group students by age and print counts of number of students at each age with more than 1 student.``() = 
            tag "Group students by age and print counts of number of students at each age with more than 1 student."
            query {
                    for student in db.Student do
                    groupBy student.Age into group
                    where (group.Count() > 1)
                    select (group.Key, group.Count())
            }
            |> Seq.iter (fun (age, ageCount) ->
                 printfn "Age: %s Count: %d" (printNullable <| Nullable age) ageCount)


        [<Test>]
        member this.``Group students by age and sum ages.``() = 
            tag "Group students by age and sum ages."
            query {
                    for student in db.Student do
                    groupBy student.Age into g        
                    let total = query { for student in g do sumByNullable (Nullable student.Age) }
                    select (g.Key, g.Count(), total)
            }
            |> Seq.iter (fun (age, count, total) ->
                printfn "Age: %d" (age)
                printfn "Count: %d" count
                printfn "Total years: %s" (total.ToString()))


        [<Test>]
        member this.``Group students by age and count number of students at each age, and display all with count > 1 in descending order of count.``() = 
            tag "Group students by age and count number of students at each age, and display all with count > 1 in descending order of count."
            query {
                    for student in db.Student do
                    groupBy student.Age into g
                    where (g.Count() > 1)        
                    sortByDescending (g.Count())
                    select (g.Key, g.Count())
            }
            |> Seq.iter (fun (age, myCount) ->
                printfn "Age: %i" age
                printfn "Count: %d" myCount)


        [<Test>]
        member this.``Select students from a set of IDs``() = 
            tag "Select students from a set of IDs"
            let idList = [1; 2; 5; 10]
            let idQuery = query { for id in idList do
                                   select id }
            query {
                    for student in db.Student do
                    where (idQuery.Contains(student.StudentId))
                    select student
                    }
            |> Seq.iter (fun student ->
                printfn "Name: %s" student.Name)


        [<Test>]
        member this.``Look for students with Name match _e%% pattern and take first two.``() = 
            tag "Look for students with Name match _e%% pattern and take first two."
            query {
                for student in db.Student do
                where (SqlMethods.Like( student.Name, "_e%") )
                select student
                take 2   
                }
            |> Seq.iter (fun student -> printfn "%s" student.Name)


        [<Test>]
        member this.``Look for students with Name matching [abc]%% pattern.``() = 
            tag "Look for students with Name matching [abc]%% pattern."
            query {
                for student in db.Student do
                where (SqlMethods.Like( student.Name, "[abc]%") )
                select student  
                }
            |> Seq.iter (fun student -> printfn "%s" student.Name)


        [<Test>]
        member this.``Look for students with name matching [^abc]%% pattern.``() = 
            tag "Look for students with name matching [^abc]%% pattern."
            query {
                for student in db.Student do
                where (SqlMethods.Like( student.Name, "[^abc]%") )
                select student  
                }
            |> Seq.iter (fun student -> printfn "%s" student.Name)


        [<Test>]
        member this.``Look for students with name matching [^abc]%% pattern and select ID.``() = 
            tag "Look for students with name matching [^abc]%% pattern and select ID."
            query {
                for n in db.Student do
                where (SqlMethods.Like( n.Name, "[^abc]%") )
                select n.StudentId    
                }
            |> Seq.iter (fun id -> printfn "%d" id)


        [<Test>]
        member this.``Using Contains as a query filter.``() = 
            tag "Using Contains as a query filter."
            query {
                    for student in db.Student do
                    where (student.Name.Contains("a"))
                    select student
                }
            |> Seq.iter (fun student -> printfn "%s" student.Name)


        [<Test>]
        member this.``Searching for names from a list.``() = 
            tag "Searching for names from a list."
            let names = [|"a";"b";"c"|]
            query {
                for student in db.Student do
                if names.Contains (student.Name) then select student }
            |> Seq.iter (fun student -> printfn "%s" student.Name)

    //     *
        [<Test>]
        member this.``Join Student and CourseSelection tables.``() = 
            tag "Join Student and CourseSelection tables."
            query {
                    for student in db.Student do 
                    join selection in db.CourseSelection 
                      on (student.StudentId = selection.StudentId)
                    select (student, selection)
                }
            |> Seq.iter (fun (student, selection) -> printfn "%d %s %d" student.StudentId student.Name selection.CourseId)


        [<Test>]
        member this.``Left Join Student and CourseSelection tables.``() = 
            tag "Left Join Student and CourseSelection tables."
            query {
                for student in db.Student do
                leftOuterJoin selection in db.CourseSelection 
                  on (student.StudentId = selection.StudentId) into result
                for selection in result.DefaultIfEmpty() do
                select (student, selection)
                }
            |> Seq.iter (fun (student, selection) ->
                let selectionID, studentID, courseID =
                    match selection with
                    | null -> "NULL", "NULL", "NULL"
                    | sel -> (sel.Id.ToString(), sel.StudentId.ToString(), sel.CourseId.ToString())
                printfn "%d %s %d %s %s %s" student.StudentId student.Name (student.Age) selectionID studentID courseID)


        [<Test>]
        member this.``Join with count``() = 
            tag "Join with count"
            query {
                    for n in db.Student do 
                    join e in db.CourseSelection on (n.StudentId = e.StudentId)
                    count        
                }
            |>  printfn "%d"


        [<Test>]
        member this.``Join with distinct.``() = 
            tag "Join with distinct."
            query {
                    for student in db.Student do 
                    join selection in db.CourseSelection on (student.StudentId = selection.StudentId)
                    distinct        
                }
            |> Seq.iter (fun (student, selection) -> printfn "%s %d" student.Name selection.CourseId)


        [<Test>]
        member this.``Join with distinct and count.``() = 
            tag "Join with distinct and count."
            query {
                    for n in db.Student do 
                    join e in db.CourseSelection on (n.StudentId = e.StudentId)
                    distinct
                    count       
                }
            |> printfn "%d"


        [<Test>]
        member this.``Selecting students with age between 10 and 15.``() = 
            tag "Selecting students with age between 10 and 15."
            query {
                    for student in db.Student do
                    where (student.Age >= 10 && student.Age < 15)
                    select student
                }
            |> Seq.iter (fun student -> printfn "%s" student.Name)


        [<Test>]
        member this.``Selecting students with age either 11 or 12.``() = 
            tag "Selecting students with age either 11 or 12."
            query {
                    for student in db.Student do
                    where (student.Age = 11 || student.Age = 12)
                    select student
                }
            |> Seq.iter (fun student -> printfn "%s" student.Name)


        [<Test>]
        member this.``Selecting students in a certain age range and sorting.``() = 
            tag "Selecting students in a certain age range and sorting."
            query {
                    for n in db.Student do
                    where (n.Age = 12 || n.Age = 13)
                    sortByNullableDescending (Nullable n.Age)
                    select n
                }
            |> Seq.iter (fun student -> printfn "%s %s" student.Name (printNullable <| Nullable student.Age))


        [<Test>]
        member this.``Selecting students with certain ages, taking account of possibility of nulls.``() = 
            tag "Selecting students with certain ages, taking account of possibility of nulls."
            query {
                    for student in db.Student do
                    where ((student.Age = 11) || (student.Age = 12))
                    sortByDescending student.Name 
                    select student.Name
                    take 2
                }
            |> Seq.iter (fun name -> printfn "%s" name)


        [<Test>]
        member this.``Union of two queries.``() = 
            tag "Union of two queries."

            let query1 = query {
                    for n in db.Student do
                    select (n.Name, n.Age)
                }

            let query2 = query {
                    for n in db.LastStudent do
                    select (n.Name, n.Age)
                    }

            query2.Union (query1)
            |> Seq.iter (fun (name, age) -> printfn "%s %s" name (printNullable<|Nullable age))


        [<Test>]
        member this.``Intersect of two queries.``() = 
            tag "Intersect of two queries."
            let query1 = query {
                    for n in db.Student do
                    select (n.Name, n.Age)
                }

            let query2 = query {
                    for n in db.LastStudent do
                    select (n.Name, n.Age)
                    }

            query1.Intersect(query2)
            |> Seq.iter (fun (name, age) -> printfn "%s %s" name (printNullable<| Nullable age))


        [<Test>]
        member this.``Using if statement to alter results for special value.``() = 
            tag "Using if statement to alter results for special value."
            query {
                    for student in db.Student do
                    select (if student.Age = -1 then
                               (student.StudentId, 100, student.Age)
                            else (student.StudentId, student.Age, student.Age))
                }
            |> Seq.iter (fun (id, value, age) -> printfn "%d %i %i" id value age)


        [<Test>]
        member this.``Using if statement to alter results special values.``() = 
            tag "Using if statement to alter results special values."
            query {
                    for student in db.Student do
                    select (if  student.Age = -1 then
                               (student.StudentId,100, student.Age)
                            elif student.Age = 0 then
                                (student.StudentId, 100, student.Age)
                            else (student.StudentId, student.Age, student.Age))
                }
            |> Seq.iter (fun (id, value, age) -> printfn "%d %i %i" id  value age)



        [<Test>]
        member this.``Multiple table select.``() = 
            tag "Multiple table select."
            query {
                    for student in db.Student do
                    for course in db.Course do
                    select (student, course)
            }
            |> Seq.iteri (fun index (student, course) ->
                if (index = 0) then printfn "StudentId Name Age CourseId CourseName"
                printfn "%d %s %i %d %s" student.StudentId student.Name student.Age course.CourseId course.CourseName)


        [<Test>]
        member this.``Multiple Joins``() = 
            tag "Multiple Joins"
            query {
                for student in db.Student do
                join courseSelection in db.CourseSelection on
                    (student.StudentId = courseSelection.StudentId)
                join course in db.Course on
                      (courseSelection.CourseId = course.CourseId)
                select (student.Name, course.CourseName)
                }
                |> Seq.iter (fun (studentName, courseName) -> printfn "%s %s" studentName courseName)


        [<Test>]
        member this.``Multiple Left Outer Joins``() = 
            tag "Multiple Left Outer Joins"
            query {
               for student in db.Student do
                leftOuterJoin courseSelection in db.CourseSelection 
                  on (student.StudentId = courseSelection.StudentId) into g1
                for courseSelection in g1.DefaultIfEmpty() do
                leftOuterJoin course in db.Course 
                  on (courseSelection.CourseId = course.CourseId) into g2
                for course in g2.DefaultIfEmpty() do
                select (student.Name, course.CourseName)
                }
            |> Seq.iter (fun (studentName, courseName) -> printfn "%s %s" studentName courseName)

