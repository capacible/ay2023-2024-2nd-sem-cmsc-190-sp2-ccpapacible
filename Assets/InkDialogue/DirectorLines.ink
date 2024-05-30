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
VAR DirectorAttractsBadPeople = false
VAR April20th = false
VAR VoicemailFromWife = false

// other optional vars
VAR TALKED_ABOUT_CONFRONTATION = false
VAR TALKED_ABOUT_GARDEN = false
VAR MUST_SOLVE_MYSTERY = false // for when director tells you to solve this once and for all
VAR DONE_ACCUSE = false // when you accuse someone
VAR CONFIRMATION_IS_BETH = false
VAR DID_NOT_PROMISE = false

// everything here initialize to true
VAR INITIAL_TALK = true

-> MAIN_THREAD

// start of conversation here
=== MAIN_THREAD ===
{INITIAL_TALK:
    ->INITIAL_MESSAGE
  - else:
    ->REGULAR_GENERIC_TALK
}

->DONE

=== REGULAR_GENERIC_TALK ===
Hello there, Enid. Need something? #display_name:Director Virgil #archetype:director
->START_THREAD_CHOICES
-> DONE

=== GENERIC_CIRCLE_BACK ===
Anything else? #display_name:Director Virgil #archetype:director
->START_THREAD_CHOICES
->DONE

=== INITIAL_MESSAGE ===
// setting the initial flag to false
~INITIAL_TALK = false
Well, if it isn't Enid. How's your vacation? By the way, can you tell the custodians to clean up the shrubs outside? Some animal messed around out front. #display_name:Director Virgil #archetype:director
    ->START_THREAD_CHOICES
-> DONE

// choices after the initial talk.
=== START_THREAD_CHOICES ===
* [{TALKED_ABOUT_CONFRONTATION == false: What in the world happened just now? }] -> ABOUT_CONFRONTATION
* [{Cassandra_BuriedTimeCapsule: I just checked the garden. Apparently Cassandra buried a time capsule out front.}] ->ABOUT_GARDEN
+ [{MUST_SOLVE_MYSTERY==true: I know who stole the plinth.}] -> ACCUSE
+ [Let's talk about the founder.] ->ABOUT_FOUNDER
* [{PlayerSawCardOfWoman: Who is Beth? Is that the name of the founder?}] ->ABOUT_BETH
* [{ DirectorStoleCassandrasKeys: Did you steal Cassandra's keys to the storage office? }]
    Now why would I do that? I have my own set of keys. ->START_THREAD_CHOICES
* [ {DirectorTrustsJonathan: It seems like you and Jonathan are close. You seem to trust him more than you ought to as his employer.} ] -> ABOUT_DIRECTOR_AND_JONATHAN
* [ {DID_NOT_PROMISE: Fine, I promise I'll tell no one.} ]
    Great! ->ABOUT_JONATHAN_REL
* [{SolvedVasePuzzle_FoundStatue:I found a statue with a gem in the storage room.}]
    That's the statue I brought in! I've brought it in about a day ago. #add_to_player:DirectorBroughtStatue #display_name:Director Virgil #archetype:director
        + + [How did you get that?] ->ABOUT_STATUE_SOURCE
->DONE

=== ABOUT_DIRECTOR_AND_JONATHAN ===
Perhaps. Is there any reason you'd think that?#display_name:Director Virgil #archetype:director
+ [ Jonathan speaks about you in a first-name basis, as much as he tries not to. ]
    HA! Oh Jonathan. He's always bad at keeping any secret. I'd like for you to keep a secret. Can you do that?#display_name:Director Virgil #archetype:director
    ++ [ Sure. ]
        -> ABOUT_JONATHAN_REL
    ++ [Can't promise that. But what is it? ]
        You'd have to promise. Get back to me if you're willing to keep what I'm going to say a secret. God knows Jonathan can't.#display_name:Director Virgil #archetype:director
        ->DONE
+ [ {DirectorAttractsBadPeople: It seems he knows enough about your relationship with { CONFIRMATION_IS_BETH: Beth } { not CONFIRMATION_IS_BETH: the Founder }.} ]
    Really? What did he say?#display_name:Director Virgil #archetype:director
    + + [ Not good things. ]
        Ah, well... He doesn't know anything about it, so it's understandable that he'd have some judgement.#display_name:Director Virgil #archetype:director
        + + + [ What doesn't he know about? ]
        ->ABOUT_JONATHAN_WORRIES
        
    + + [ He's just concerned about you. ]
        I'm perfectly fine. He's always been a worrier.#display_name:Director Virgil #archetype:director
        + + + [ Is there a reason for him to be worried? At least, from the outside looking in? ]
            ->ABOUT_JONATHAN_WORRIES
+ [ He told me himself. What is the background between you two? ]
    ->ABOUT_JONATHAN_REL
    
->DONE

    === ABOUT_JONATHAN_REL
    Jonathan and I go way back. He was a close college friend of mine, but he dropped out because of some troubles. He's always been interested in history, but he had no credentials. Couldn't find a job.#display_name:Director Virgil #archetype:director
        He needed a job because he's fallen on hard times, so I wanted to help him out a bit.#display_name:Director Virgil #archetype:director
        -> JONATHAN_QUESTIONS
        
        = JONATHAN_QUESTIONS
        + [ Jonathan is interested in history? ]
            -> JONATHAN_INTEREST
        + [ Jonathan dropped out? ]
            -> JONATHAN_DROPPED_OUT
        + [ Jonathan has fallen on hard times? ]
            -> JONATHAN_STRUGGLES
        + [I have other questions ]
            Alright then, shoot.#display_name:Director Virgil #archetype:director 
            ->START_THREAD_CHOICES
        -> DONE
        
        = JONATHAN_INTEREST
        Oh, yes. He has always had an interest in studying history. I guess, it'd be more specific to say that he wanted to study artifacts like you do.#display_name:Director Virgil #archetype:director #effect_add_to_player:JonathansInterest_Artifacts
        When we were younger he'd always talk about it.#display_name:Director Virgil #archetype:director
        + [ Do you think he'd want to keep the plinth for himself? ]
            Absolutely not. That's not who he is. Like you, he has too much respect for the background of these objects, and would want to share it with the world.#display_name:Director Virgil #archetype:director 
            ->JONATHAN_QUESTIONS
        + [ Is the Aurora another one of his interests? ]
            I suppose you could say that. But to be honest, in any exhibit that we put together, I would always find Jonathan searching for more information about it.#display_name:Director Virgil #archetype:director
            The Aurora isn't special in that regard.#display_name:Director Virgil #archetype:director
            ->JONATHAN_QUESTIONS
        
        -> DONE
        
        = JONATHAN_DROPPED_OUT
        He did. I won't tell you anything about what happened or why he did so, as I don't know the details myself.#display_name:Director Virgil #archetype:director
        ->JONATHAN_QUESTIONS
        -> DONE
        
        =JONATHAN_STRUGGLES
        He's been struggling to make DONEs meet recently. He's always been doing the odd job here and there ever since he dropped out, but it seems the rising prices have been difficult to keep up with.#display_name:Director Virgil #archetype:director
        I've been contemplating giving him a raise, but he doesn't really perform exceptionally as a security guard. I can't go around giving raises to people I'm close with.#display_name:Director Virgil #archetype:director
        That's going to make me appear like I have favoritism.#display_name:Director Virgil #archetype:director
        -> JONATHAN_QUESTIONS
        ->DONE
    
    -> DONE
    
    === ABOUT_JONATHAN_WORRIES
    He thinks I'm ruining my marriage, running after { CONFIRMATION_IS_BETH: Beth } { not CONFIRMATION_IS_BETH: the Founder}. He thinks she's a bad person.#display_name:Director Virgil #archetype:director
    I'm not ruining anything at all. She's the sole person that funds the museum, of course I will help her around with things. I need the museum to keep going.#display_name:Director Virgil #archetype:director
        + [ {VoicemailFromWife and April20th: The voicemail from your wife implies the opposite.} ]
            What voicemail? You shouldn't snoop around anyone's business when it's not pertinent, you know. I could fire you for that. #display_name:Director Virgil #archetype:director
            -> GENERIC_CIRCLE_BACK
        + [ Don't you think you might be doing too much? ]
            Oh, absolutely not. I mean, if the founder thinks it's too much, then she'd stop me.#display_name:Director Virgil #archetype:director
            + + [ I guess so... ]
                -> GENERIC_CIRCLE_BACK
            + + [ Sure... ]
                -> GENERIC_CIRCLE_BACK
    ->DONE


=== ABOUT_CONFRONTATION ===
// all here
Oh, well, Cassandra was just telling me to show her where the plinth is. Apparently, it was missing! #display_name:Director Virgil #archetype:director

->ABOUT_CONFRONTATION_CHOICES

    = ABOUT_CONFRONTATION_CHOICES
    * [Yes, it's missing.]
        Well, isn't that delightful, the founder will kill me for that.#display_name:Director Virgil #archetype:director
        -> ABOUT_CONFRONTATION_CHOICES
    * [Did you do anything to the plinth last night?] -> QUESTION_DIRECTOR
    ->DIRECTORS_REQUEST
    

    = QUESTION_DIRECTOR
    What are you on about? Of course not. I've been too busy looking for the statue it's supposed to be attached to.  #display_name:Director Virgil #archetype:director
    -> DIRECTORS_REQUEST

~TALKED_ABOUT_CONFRONTATION = true

->DONE

=== DIRECTORS_REQUEST ===
~MUST_SOLVE_MYSTERY = true
Could you do me a favor, Enid? Can you figure out what happened to the plinth and get back to me? This whole mess is giving me a headache. #display_name:Director Virgil #archetype:director

+ [ Let's talk about something else. ] ->GENERIC_CIRCLE_BACK

->DONE

=== ABOUT_STATUE_SOURCE ===
I received the statue from the founder. Apparently, before the excavation, some kid had snuck into the site and took the 'prettiest thing on the site.' It got sold for a ridiculously high price.#display_name:Director Virgil #archetype:director 
->ABOUT_STATUE_SOURCE_FOUNDER
= ABOUT_STATUE_SOURCE_FOUNDER
* [ The founder bought it? ]
    Yes and no. One of her friends bought it during the auction, and when the founder heard that we're putting together an exhibit on The Arbiter and The Aurora, well... let's just say she managed to buy it from that friend. #display_name:Director Virgil #archetype:director
    -> ABOUT_STATUE_SOURCE_FOUNDER
* [ How did it get into the founder's hands? ]
    Another friend of the founder bought it off that auction. When she heard we were putting together an exhibit on The Arbiter and The Aurora, she went and convinced her friend to sell it to her.#display_name:Director Virgil #archetype:director
    The founder donated the statue to us. We should be grateful. #display_name:Director Virgil #archetype:director
    ->ABOUT_STATUE_SOURCE_FOUNDER
* [ So you didn't steal the plinth, like Cassandra says? ]
    Goodness no! Why would I do that?... sigh. #display_name:Director Virgil #archetype:director
    ->DIRECTORS_REQUEST

->DONE

=== ABOUT_BETH ===
Ah, Beth... Yes, Beth is the founder. How did you know her name, by the way? #effect_add_to_player:FounderIdentity_Beth_Confirmed #display_name:Director Virgil #archetype:director
~CONFIRMATION_IS_BETH=true
+ [I opened your safe. I'll have you know that it's because I was trying to find the plinth.]
    Ah. I'll let it slide this once. #display_name:Director Virgil #archetype:director
        + +  Why does she visit so often? The frequency was enough for the staff to recall all the gifts she brought.  -> ABOUT_DIRECTOR_VISITOR_REASON
+ [ (Lie) I heard other people talk about her as I investigated.]
    You heard other people talk about her? Who? As far as I know, no one knows of her personally.#display_name:Director Virgil #archetype:director
        + +  Why does she visit so often? The frequency was enough for the staff to recall all the gifts she brought.  -> ABOUT_DIRECTOR_VISITOR_REASON
->DONE

=== ABOUT_DIRECTOR_VISITOR ===
...Yes. She funds the museum, checks on what's going on, and sees if there are any... artifacts of interest she could buy.#display_name:Director Virgil #archetype:director
-> ABOUT_FOUNDER
->DONE

=== ABOUT_DIRECTOR_VISITOR_REASON ===
She often visits to check on the state of the museum. She's a good person, you know. The entire museum was built because of her.#display_name:Director Virgil #archetype:director
+ [ { SellThePlinth_Founder and DirectorsOpinion_SellThePlinth: Was she the person you initially wanted to sell the plinth to?  } ]
    ...Yes, that was what I initially wanted. The founder--Beth--had a particular interest in the plinth, so I was thinking of letting her buy it.#display_name:Director Virgil #archetype:director
    However, her interest in acquiring it reversed into finding the statue for us. #display_name:Director Virgil #archetype:director
    ->START_THREAD_CHOICES
+ [ { KeyInDirectorsOffice && not KeyInDrectorsOffice_Cassandras: Does the key I found in your office happen to be hers? } ]
    Hmmm... I don't think so. When Beth loses something, she's quick to catch on. It's not hers.#display_name:Director Virgil #archetype:director
    Have you considered it to be Cassandra's? Perhaps she dropped it during one of my meetings. That appears to be a storage room key. #display_name:Director Virgil #archetype:director
    ->START_THREAD_CHOICES
+ -> START_THREAD_CHOICES

-> DONE

=== ABOUT_SELLING_PLINTH ===
Alright, you caught me. I was thinking of selling. The Aurora expedition had a lot more interesting artifacts, and Beth wanted to buy it for an insanely high price. I thought maybe the museum could use that little extra funding.#display_name:Director Virgil #archetype:director
+ [ {SolvedVasePuzzle_FoundStatue: I found a statue that looks connected to the plinth in the storage room. }]
   Yes, when Beth brought that to me, I changed my mind about selling. I've brought it in about a day ago. #add_to_player:DirectorBroughtStatue #display_name:Director Virgil #archetype:director
   -> ABOUT_STATUE_SOURCE

-> DONE

=== ABOUT_FOUNDER ===
What about her?#display_name:Director Virgil #archetype:director
+ [ {PlayerSawCardOfWoman: Is she Beth?} ] -> ABOUT_BETH
+ [ {RichWomanAndDirector: Is she the woman who drops by all the time?}] -> ABOUT_DIRECTOR_VISITOR
+ [ {SellThePlinth_Founder: I heard you were planning on selling the plinth to the founder. Is that true?} ] -> ABOUT_SELLING_PLINTH
+ [ I think the plinth was stolen by the founder. ] -> ABOUT_PLINTH_AND_FOUNDER
-> DONE

=== ABOUT_PLINTH_AND_FOUNDER ===
No. I don't think she did. Do you have any proof?#display_name:Director Virgil #archetype:director
-> START_THREAD_CHOICES
->DONE

=== ABOUT_GARDEN ===
~TALKED_ABOUT_GARDEN = true
Why would she do that? #display_name:Director Virgil #archetype:director
+ [{TimeCapsule_Explanation: She loves the museum, you know. Enough to want what's best for it} Talk to her yourself, if you want.] ->TIME_CAPSULE
+ [No idea, to be honest. You'll have to ask her yourself.] ->TIME_CAPSULE

-> DONE

=== TIME_CAPSULE ===
I would, but you know she is angry with me. Haven't you seen her sling accusations at me just now? #display_name:Director Virgil #archetype:director
->START_THREAD_CHOICES
->DONE

=== ACCUSE ===
Who took the plinth? #display_name:Director Virgil #archetype:director #accuse:true
* [Cassandra.] 
    ~DONE_ACCUSE = true
    ->DONE
* [Jonathan.] 
    ~DONE_ACCUSE = true
    -> DONE
* [You.] 
    ~DONE_ACCUSE = true
    -> DONE

->DONE

