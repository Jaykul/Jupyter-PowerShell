---
theme : "League"
transition: "slide"
---

# Literate DevOps
## with Jupyter
## & PowerShell

Some thoughts and tools

by Joel "Jaykul" Bennett

---

# Who Am I?

* Hacker and Programmer <!-- .element: class="fragment" -->
* Social Science Major <!-- .element: class="fragment" -->
* Automation Specialist <!-- .element: class="fragment" -->
* Software Engineer <!-- .element: class="fragment" -->
* DevOps Consultant <!-- .element: class="fragment" -->
* Microsoft PowerShell MVP <!-- .element: class="fragment" -->


<a href="#" class="navigate-down" /> </a>

--

# Who Am I?

So in other words

```text
    I'm software engineer with a sociology background,
    doing #DevOps in the Windows (and .NET) world,
    wishing we'd stop taking the learning curve for granted.
```

<a href="#/2" class="navigate-up"></a>
---

## [Literate Programming](http://literateprogramming.com/)

A 1984 book by Donald Knuth<br/>
Goal: significantly better documentation

1. Explain to humans what we want the computer to do
2. Write as an essayist, carefully naming and explaining
3. Order the program for human comprehension

note:

I believe that the time is ripe for significantly better documentation of programs, and that we can best achieve this by considering programs to be works of literature. Hence, my title: "Literate Programming."

Let us change our traditional attitude to the construction of programs: Instead of imagining that our main task is to instruct a computer what to do, let us concentrate rather on explaining to human beings what we want a computer to do.

The practitioner of literate programming can be regarded as an essayist, whose main concern is with exposition and excellence of style. Such an author, with thesaurus in hand, chooses the names of variables carefully and explains what each variable means. He or she strives for a program that is comprehensible because its concepts have been introduced in an order that is best for human understanding, using a mixture of formal and informal methods that reinforce each other.


--
## Literate Programming

Tools (tangle and weave) were written...

But the examples you'll find today are all demos, parts of a book or article on the topic.

Needless to say, it never really took off.

### So why bring it up?<!-- .element: class="fragment fade-in" -->

note:

Writing a literate program is a lot more work than writing a normal program. After all, who ever documents their programs in the first place!? Moreover, who documents them in a pedagogical style that is easy to understand? And finally, who _ever_ provides commentary on the theory and design issues behind the code as they write the documentation?


--
## Literate Programming

> I believe that the time is ripe for significantly better documentation

There are new tools, new audiences

* Emacs Org Mode
* IPython => Jupyter
* nteract, Spyder IDE, Atom Hydrogen
* Apache Zeppelin
* Jupyter Labs

---
## Literate DevOps

[Howard Abrams](http://www.howardism.org/Technical/Emacs/literate-devops.html) and [Marc Hoffman](https://archive.fosdem.org/2016/schedule/event/literate_devops_for_configuration_management/)<br/> like to talk about DevOps as bi-modal:

1. Bang head until server works
2. Capture effort into automation


We want to make 1. more like 2.<!-- .element: class="fragment fade-in" -->

--

## Literate DevOps

We want to capture the process of investigating and learning so that:

1. Others can learn from our bruises
2. We can export it to the automation

---

## Juptyer Notebooks

Since the goal is to capture the investigation,<br/>
 I basically have to do one of two things:

1. Take copious notes
2. Enable transcription

<br/>

For now, let's talk about taking notes<br />
(but don't forget to ask me about transcription later)<!-- .element: class="fragment fade-in" -->

--
## Jupyter Notebooks

**Remember** this is _not_ for application code, it's
* Infrastructure
* Deployments

I'm not writing an essay about my grandiose plans (yet), but I am simply taking notes during my investigation. I need a tool that's good for that.

* I need a notebook
  * I like markdown
  * I want code inline
* But I



--
# Jupyter Demo

Allow me to introduce Jupyter
