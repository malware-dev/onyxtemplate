# Onyx Template

Onyx Template is a very simple Source Generator powered runtime text template system.

- Note: Only really tested with modern project format + modern nuget. No guarantees of  
  proper function in old .NET Framework projects etc.

---
## Usage:
1. Add the nuget package.
2. Add a file with an `.onyx` extension.  
   (It _should_ be added automatically as AdditionalFiles. If not - change it to that).

The macros are fairly simple.

`{{ YourMacroName }}`

Let's create our first .onyx file. Let's call it `WelcomeText.onyx`.

```
Hello, {{ UserName }}, welcome to the show.
```

The source generator will have generated a base class for you to derive from. Create a
new `.cs` file in the same folder, named `WelcomeText.onyx.cs` (or whatever you want,
it's just useful to group them together).

```csharp
class WelcomeText: WelcomeTextBase
{
    protected override string GetUserName()
    {
        return "Batman";
    }
}
```
Every macro you make in your .onyx file will get its own Get method you can override
to provide the replacement string for that macro.

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
class AnimalList: AnimalListBase
{
    protected override IEnumerable<string> GetAnimals()
    {
        yield return "Dog";
        yield return "Cat";
        yield return "Tiger";
        yield return "Bear";
    }
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
class AnimalList: AnimalListBase
{
    protected override IEnumerable<string> GetAnimals()
    {
        yield return "Dog\n- Canidae";
        yield return "Cat\n- Felidae";
        yield return "Tiger\n- Felidae";
        yield return "Bear\n- Ursidae";
    }
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
class AnimalList: AnimalListBase
{
    class Animal : AnimalsItemBase
    {
        readonly string _name;
        readonly string _type;

        public Animal(string name, string type)
        {
            _name = name;
            _type = type;
        }

        public override string GetName() => _name;
        public override string GetType() => _type;
    }
    
    protected override IEnumerable<AnimalsItemBase> GetAnimals()
    {
        yield return new Animal("Dog", "Canidae");
        yield return new Animal("Cat", "Felidae");
        yield return new Animal("Tiger", "Felidae");
        yield return new Animal("Bear", "Felidae");
    }
}
```
The generator has produced an item base class we can derive from to
produce data for the complex macro. So we make an `Animal` class, which
derives from the generated `AnimalsItemBase` class, and our `GetAnimals()`
class is changed to return a set of those instead of a simple string.

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
   Animal Family: Felidae

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

## Using the template
Instantiate your generated template, and fill the properties with the values you need.

```csharp
var instance = new ThatTemplate();
instance.Items = new[] 
{
    new ThatTemplate.MyListItem() { Value1 = "Hello", Value2 = "World" },
    new ThatTemplate.MyListItem() { Value1 = "Hei", Value2 = "Verden" },
    new ThatTemplate.MyListItem() { Value1 = "Hallo", Value2 = "Welt" }
};
instance.ATitleOrSomething = "World Greetings";

var result = instance.ToString();
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
