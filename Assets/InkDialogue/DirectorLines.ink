// variables for determining various lines
VAR DirectorsFrequentVisitor = false
VAR RichWomanAndDirector = false
VAR WhoIsBeth = false
VAR PlayerSawCardOfWoman = false
VAR DirectorTrustsJonathan = false
VAR KeyInDirectorsOffice = false
VAR KeyInDrectorsOffice_Cassandras = false
VAR DirectorsOpinion_PlinthIsUnimportant = false
VAR DirectorsOpinion_SellThePlinth = false
VAR SellThePlinth_Founder = false
VAR DirectorStoleCassandrasKeys = false
VAR DirectorLikesFounder = false
VAR Cassandra_BuriedTimeCapsule = false
VAR TimeCapsule_Explanation = false
VAR Cassandra_TensionWithDirector = false
VAR SolvedVasePuzzle_FoundStatue = false

// other optional vars
VAR TALKED_ABOUT_CONFRONTATION = false
VAR TALKED_ABOUT_GARDEN = false
VAR MUST_SOLVE_MYSTERY = false // for when director tells you to solve this once and for all

// everything here initialize to true
VAR INITIAL_TALK = true

-> MAIN_THREAD

// start of conversation here
=== MAIN_THREAD ===
{INITIAL_TALK: -> INITIAL_MESSAGE }
-> REGULAR_GENERIC_TALK
->END

=== REGULAR_GENERIC_TALK ===
Hello there, Enid. Need something? #display_name:Director Virgil
->START_THREAD_CHOICES
-> END

=== INITIAL_MESSAGE ===
Well, if it isn't Enid. How's your vacation? By the way, can you tell the custodians to clean up the shrubs outside? Some animal messed around out front. #display_name:Director Virgil
    ->START_THREAD_CHOICES
// setting the initial flag to false
~INITIAL_TALK = false
-> END

// choices after the initial talk.
=== START_THREAD_CHOICES ===
* [{TALKED_ABOUT_CONFRONTATION == false: What in the world happened just now? }] -> ABOUT_CONFRONTATION
+ -> ABOUT_FOUNDER
* [{Cassandra_BuriedTimeCapsule: I just checked the garden. Apparently Cassandra buried a time capsule out front.}] ->ABOUT_GARDEN
+ [{MUST_SOLVE_MYSTERY: I know who stole the plinth.}] -> ACCUSE
+ [Let's talk about the founder.] ->ABOUT_FOUNDER
+ [{PlayerSawCardOfWoman: Who is Beth? Is that the name of the founder?}] ->ABOUT_BETH
->END

=== ABOUT_CONFRONTATION ===
// all here
Oh, well, Cassandra was just telling me to show her where the plinth is. Apparently, it was missing! #display_name:Director Virgil

->ABOUT_CONFRONTATION_CHOICES

    = ABOUT_CONFRONTATION_CHOICES
    * [Yes, it's missing.]
        Well, isn't that delightful, the founder will kill me for that.  #display_name:Director Virgil
        -> ABOUT_CONFRONTATION_CHOICES
    * [Did you do anything to the plinth last night?] -> QUESTION_DIRECTOR
    ->DIRECTORS_REQUEST
    

    = QUESTION_DIRECTOR
    What are you on about? Of course not. I've been too busy looking for the statue it's supposed to be attached to.  #display_name:Director Virgil
            * [{SolvedVasePuzzle_FoundStatue:You mean the statue with the gem? I found it in the storage room}]
                Yes, that's the one! I've brought it in about a day ago. #add_to_player:DirectorBroughtStatue #displayName:Director Virgil
                    + + [How did you get that?] ->ABOUT_STATUE_SOURCE
    -> DIRECTORS_REQUEST
    


~TALKED_ABOUT_CONFRONTATION = true

->END

=== DIRECTORS_REQUEST ===
Could you do me a favor, Enid? Can you figure out what happened to the plinth and get back to me? This whole mess is giving me a headache. #display_name:Director Virgil
~MUST_SOLVE_MYSTERY = true

+ [ Let's talk about something else. ] ->START_THREAD_CHOICES

->END

=== ABOUT_STATUE_SOURCE ===
I received the statue from the founder. Apparently, before the excavation, some kid had snuck into the site and took the 'prettiest thing on the site.' It got sold for a ridiculously high price.#display_name:Director Virgil 
->ABOUT_STATUE_SOURCE_FOUNDER
= ABOUT_STATUE_SOURCE_FOUNDER
* [ The founder bought it? ]
    Yes and no. One of her friends bought it during the auction, and when the founder heard that we're putting together an exhibit on The Arbiter and The Aurora, well... let's just say she managed to buy it from that friend. #display_name:Director Virgil
    -> ABOUT_STATUE_SOURCE_FOUNDER
* [ How did it get into the founder's hands? ]
    Another friend of the founder bought it off that auction. When she heard we were putting together an exhibit on The Arbiter and The Aurora, she went and convinced her friend to sell it to her.
    The founder donated the statue to us. We should be grateful. #display_name:Director Virgil
    ->ABOUT_STATUE_SOURCE_FOUNDER
* [ So you didn't steal the plinth, like Cassandra says? ]
    Goodness no! Why would I do that?... sigh. #display_name:Director Virgil
    ->DIRECTORS_REQUEST

->END

=== ABOUT_BETH ===
Ah, Beth... {MUST_SOLVE_MYSTERY: if you must ask this to solve our problem, then yes, Beth is the founder.} {not MUST_SOLVE_MYSTERY: Yes, Beth is the founder.} How did you know her name, by the way?
+ [I opened your safe. I'll have you know that it's because I was trying to find the plinth.]
    Ah. I'll let it slide this once. ->DONE
+ [ (Lie) I heard other people talk about her as I investigated.]
    You heard other people talk about her? Who? As far as I know, no one knows of her personally.->DONE
->END

=== ABOUT_DIRECTOR_VISITOR ===
->END

=== ABOUT_SELLING_PLINTH ===
-> END

=== ABOUT_FOUNDER ===
What about her?
+ [ {PlayerSawCardOfWoman: Is she Beth?} ] -> ABOUT_BETH
+ [ {RichWomanAndDirector: Is she the woman who drops by all the time?}] -> ABOUT_DIRECTOR_VISITOR
+ [ {SellThePlinth_Founder: I heard you were planning on selling the plinth to the founder. Is that true?} {Cassandra_TensionWithDirector: Is that why you were having issues with Cassandra?} ] -> ABOUT_SELLING_PLINTH
+ [ I think the plinth was stolen by the founder. ] -> ABOUT_PLINTH_AND_FOUNDER
-> END

=== ABOUT_PLINTH_AND_FOUNDER ===
->END

=== ABOUT_GARDEN ===
~TALKED_ABOUT_GARDEN = true
Why would she do that? #display_name:Director Virgil
+ [{TimeCapsule_Explanation: She loves the museum, you know. Enough to want what's best for it} Talk to her yourself, if you want.] ->TIME_CAPSULE
+ [No idea, to be honest. You'll have to ask her yourself.]

I would, but you know she is angry with me. Haven't you seen her sling accusations at me just now? #display_name:Director Virgil
-> END

=== TIME_CAPSULE ===
->END

=== ACCUSE ===
Who took the plinth? #display_name:Director Virgil
* [Cassandra.] -> FINISH_GAME_CASS
* [Jonathan.] -> FINISH_GAME_JONATHAN
* [You.] -> FINISH_GAME_DIRECTOR

->END

=== FINISH_GAME_CASS ===
->END


=== FINISH_GAME_JONATHAN ===
->END


=== FINISH_GAME_DIRECTOR ===
->END