# Onyx Template

[![Build and Deploy](https://github.com/malware-dev/onyxtemplate/actions/workflows/deploynuget.yml/badge.svg?branch=main)](https://github.com/malware-dev/onyxtemplate/actions/workflows/deploynuget.yml)

Onyx Template is a very simple Source Generator powered runtime text template system.

- Note: Only really tested with modern project format + modern nuget. No guarantees of  
  proper function in old .NET Framework projects etc.

---
## Usage:
1. Add the nuget package Mal.OnyxTemplate.
2. Add a file with an `.onyx` extension.  
   (It _should_ be added automatically as AdditionalFiles. If not - change it to that).

The macros are fairly simple.

`{{ YourMacroName }}`

Let's create our first .onyx file. Let's call it `WelcomeText.onyx`.

```
Hello, {{ UserName }}, welcome to the show.
```

The source generator will have generated a class for you. Every macro you make in your .onyx file will get its own 
property string (or boolean) you can set for that macro.

---

### ... But I need it to be public / $template
There is a special configuration macro you can add to the very top of your .onyx file
to configure it. Let's mke our WelcomeText macro generate a `public` base class
instead of the default `internal`.

```
{{ $template public }}
Hello, {{ UserName }}, welcome to the show.
```

That's all it takes.

---

## Repeating data / $foreach
To generate lists of data you need to utilize the special `$foreach` macro:
```
Animals:
{{ $foreach animal in animals }}   
   {{ animal }}
{{ $next }}
```
Providing we give it the following data provider:
```csharp
var template = new Template();
template.Animals = new[] {
    "Dog",
    "Cat",
    "Tiger",
    "Bear"
}
```
The result will be
```
Animals:
   Dog
   Cat
   Tiger
   Bear
```

_See also [item state conditionals](#item-state-conditionals)_

---

## Dealing with multiline macros
But what if the `animal` macro returned multiple lines? Let's try it:
```csharp
var template = new Template();
template.Animals = new[] {
    "Dog\n- Canidae",
    "Cat\n- Felidae",
    "Tiger\n- Felidae",
    "Bear\n- Ursidae"
}
```
Result:
```
Animals:
   Dog
- Canidae
   Cat
- Felidae
   Tiger
- Felidae
   Bear
- Ursidae
```
So yes, that worked, but... it's not _exactly_ what we're after, is it. We'd
prefer it if the new lines were aligned with the first macro.

There's two ways we can solve this problem. We can set an `indented` flag
in the template header
```
{{ $template indented }}
```
_or_ we can tell the generator that only this particular use of the macro
needs to be indented:
```
   {{ animal:indent }}
```
Result:
```
Animals:
   Dog
   - Canidae
   Cat
   - Felidae
   Tiger
   - Felidae
   Bear
   - Ursidae
```
That's better!

---

## Complex macros
We could make it even better though. Rather than formatting the whole thing
in the data provider itself we could do this:
```
Animals:

{{ $foreach animal in animals }}   
   Animal Name:   {{ name }}
   Animal Family: {{ type }}

{{ $next }}
```
Now this is detected to be a complex macro. This means we'll have to change our
data provider again:
```csharp
var template = new Template();
template.Animals = new[] 
{
    new Template.Animal { Name = "Dog", Type = "Canidae" },
    new Template.Animal { Name = "Cat", Type = "Felidae" },
    new Template.Animal { Name = "Tiger", Type = "Felidae" },
    new Template.Animal { Name = "Bear", Type = "Ursidae" }
}
```
The generator has produced an item class we can use to produce data for the complex 
macro.

Now, our results are:
```
Animals:
   
   Animal Name:   Dog
   Animal Family: Canidae
   
   Animal Name:   Cat
   Animal Family: Felidae
   
   Animal Name:   Tiger
   Animal Family: Felidae
   
   Animal Name:   Bear
   Animal Family: Ursidae

```
---

## Conditionals
Sometimes you want templates that may or may not generate parts of itself 
based on a condition. We can do that by using the `{{ $if }}`, `{{ $elseif }}`,
`{{ $else }}` macros.
```
Mr. Jenkins, you're {{ $if isfired }}fired!{{ $else }}hired.{{ $end }}
```
You can invert a condition by including the `not` modifier
```
Mr. Jenkins, you're {{ $if not isfired }}hired.{{ $else }}fired!{{ $end }}
```

### Item state conditionals
You can query a set of states about items in a [`$foreach`](#repeating-data-foreach) 
by using a specialized set of fields:
- `$first` The item is the first item of the list.
- `$last` The item is the last item of the list.
- `$middle` The item is neither the first nor the last item in the list.
- `$odd` The item is of odd numbered index.
- `$even` The item is of even numbered index.

```
{{ $foreach item in list }}
    item.name{{ $if not $last }},{{ $end }}
{{ $next}}
```

---

## And that's it
This library was primarily designed 1. to help me get better at making source
generators and 2. because I needed something simple to make my _other_ source
generators cleaner, and without all the StringBuilder shenanigans all over
the place.

### Known Issue with Jetbrains Rider
There seems to be some kind of caching going on with Rider which prevents it
from detecting changes in the .onyx files now and again. I have yet to find
a solution for it. A Rebuild All forces the issue.

### Watch your whitespace
If you have whitespace before or after the `{{ $template }}`,
`{{ $foreach item in source }}` and `{{ $next }}` macros, you might get
newlines you don't want.
