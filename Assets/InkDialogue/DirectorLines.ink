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
{INITIAL_TALK: -> INITIAL_MESSAGE }
-> REGULAR_GENERIC_TALK
->END

=== REGULAR_GENERIC_TALK ===
Hello there, Enid. Need something? #display_name:Director Virgil
->START_THREAD_CHOICES
-> END

=== GENERIC_CIRCLE_BACK ===
Anything else?
->START_THREAD_CHOICES
->END

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
* [{PlayerSawCardOfWoman: Who is Beth? Is that the name of the founder?}] ->ABOUT_BETH
* [{ DirectorStoleCassandrasKeys: Did you steal Cassandra's keys to the storage office? }]
    Now why would I do that? I have my own set of keys. ->START_THREAD_CHOICES
* [ {DirectorTrustsJonathan: It seems like you and Jonathan are close. You seem to trust him more than you ought to as his employer.} ] -> ABOUT_DIRECTOR_AND_JONATHAN
* [ {DID_NOT_PROMISE: Fine, I promise I'll tell no one.} ]
    Great! ->ABOUT_JONATHAN_REL
->END

=== ABOUT_DIRECTOR_AND_JONATHAN ===
Perhaps. Is there any reason you'd think that?
+ [ Jonathan speaks about you in a first-name basis, as much as he tries not to. ]
    HA! Oh Jonathan. He's always bad at keeping any secret. I'd like for you to keep a secret. Can you do that?
    ++ [ Sure. ]
        -> ABOUT_JONATHAN_REL
    ++ [Can't promise that. But what is it? ]
        You'd have to promise. Get back to me if you're willing to keep what I'm going to say a secret. God knows Jonathan can't.->END
+ [ {DirectorAttractsBadPeople: It seems he knows enough about your relationship with { CONFIRMATION_IS_BETH: Beth } { not CONFIRMATION_IS_BETH: the Founder }.} ]
    Really? What did he say?
    + + [ Not good things. ]
        Ah, well... He doesn't know anything about it, so it's understandable that he'd have some judgement.
        + + + [ What doesn't he know about? ]
        ->ABOUT_JONATHAN_WORRIES
        
    + + [ He's just concerned about you. ]
        I'm perfectly fine. He's always been a worrier.
        + + + [ Is there a reason for him to be worried? At least, from the outside looking in? ]
            ->ABOUT_JONATHAN_WORRIES
+ [ He told me himself. What is the background between you two? ]
    ->ABOUT_JONATHAN_REL
    
->END

    === ABOUT_JONATHAN_REL
    Jonathan and I go way back. He was a close college friend of mine, but he dropped out because of some troubles. He's always been interested in history, but he had no credentials. Couldn't find a job.
        He needed a job because he's fallen on hard times, so I wanted to help him out a bit.
        -> JONATHAN_QUESTIONS
        
        = JONATHAN_QUESTIONS
        + [ Jonathan is interested in history? ]
            -> JONATHAN_INTEREST
        + [ Jonathan dropped out? ]
            -> JONATHAN_DROPPED_OUT
        + [ Jonathan has fallen on hard times? ]
            -> JONATHAN_STRUGGLES
        + [I have other questions ]
            Alright then, shoot. ->START_THREAD_CHOICES
        -> END
        
        = JONATHAN_INTEREST
        Oh, yes. He has always had an interest in studying history. I guess, it'd be more specific to say that he wanted to study artifacts like you do. #effect_add_to_player:JonathansInterest_Artifacts
        When we were younger he'd always talk about it.
        + [ Do you think he'd want to keep the plinth for himself? ]
            Absolutely not. That's not who he is. Like you, he has too much respect for the background of these objects, and would want to share it with the world. ->JONATHAN_QUESTIONS
        + [ Is the Aurora another one of his interests? ]
            I suppose you could say that. But to be honest, in any exhibit that we put together, I would always find Jonathan searching for more information about it.
            The Aurora isn't special in that regard.
            ->JONATHAN_QUESTIONS
        
        -> END
        
        = JONATHAN_DROPPED_OUT
        He did. I won't tell you anything about what happened or why he did so, as I don't know the details myself.
        ->JONATHAN_QUESTIONS
        -> END
        
        =JONATHAN_STRUGGLES
        He's been struggling to make ends meet recently. He's always been doing the odd job here and there ever since he dropped out, but it seems the rising prices have been difficult to keep up with.
        I've been contemplating giving him a raise, but he doesn't really perform exceptionally as a security guard. I can't go around giving raises to people I'm close with.
        That's going to make me appear like I have favoritism.
        -> JONATHAN_QUESTIONS
        ->END
    
    -> END
    
    === ABOUT_JONATHAN_WORRIES
    He thinks I'm ruining my marriage, running after { CONFIRMATION_IS_BETH: Beth } { not CONFIRMATION_IS_BETH: the Founder}. He thinks she's a bad person.
    I'm not ruining anything at all. She's the sole person that funds the museum, of course I will help her around with things. I need the museum to keep going.
        + [ {VoicemailFromWife and April20th: The voicemail from your wife implies the opposite.} ]
            What voicemail? You shouldn't snoop around anyone's business when it's not pertinent, you know. I could fire you for that. -> GENERIC_CIRCLE_BACK
        + [ Don't you think you might be doing too much? ]
            Oh, absolutely not. I mean, if the founder thinks it's too much, then she'd stop me.
            + + [ I guess so... ]
                -> GENERIC_CIRCLE_BACK
            + + [ Sure... ]
                -> GENERIC_CIRCLE_BACK
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
Ah, Beth... {MUST_SOLVE_MYSTERY: if you must ask this to solve our problem, then yes, Beth is the founder.} {not MUST_SOLVE_MYSTERY: Yes, Beth is the founder.} How did you know her name, by the way? #effect_add_to_player:FounderIdentity_Beth_Confirmed
~CONFIRMATION_IS_BETH=true
+ [I opened your safe. I'll have you know that it's because I was trying to find the plinth.]
    Ah. I'll let it slide this once. 
        + + [ Why does she visit so often? The frequency was enough for the staff to recall all the gifts she brought. ] -> ABOUT_DIRECTOR_VISITOR_REASON
+ [ (Lie) I heard other people talk about her as I investigated.]
    You heard other people talk about her? Who? As far as I know, no one knows of her personally.
        + + [ Why does she visit so often? The frequency was enough for the staff to recall all the gifts she brought. ] -> ABOUT_DIRECTOR_VISITOR_REASON
->END

=== ABOUT_DIRECTOR_VISITOR ===

->END

=== ABOUT_DIRECTOR_VISITOR_REASON ===
She often visits to check on the state of the museum. She's a good person, you know. The entire museum was built because of her.
+ [ { SellThePlinth_Founder and DirectorsOpinion_SellThePlinth: Was she the person you initially wanted to sell the plinth to?  } ]
    ...Yes, that was what I initially wanted. The founder--Beth--had a particular interest in the plinth, so I was thinking of letting her buy it.
    However, her interest in acquiring it reversed into finding the statue for us. ->START_THREAD_CHOICES
+ [ { KeyInDirectorsOffice && not KeyInDrectorsOffice_Cassandras: Does the key I found in your office happen to be hers? } ]
    Hmmm... I don't think so. When Beth loses something, she's quick to catch on. It's not hers.
    Have you considered it to be Cassandra's? Perhaps she dropped it during one of my meetings. That appears to be a storage room key. ->START_THREAD_CHOICES
+ -> START_THREAD_CHOICES

-> END

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
Who took the plinth? #display_name:Director Virgil #accuse:true
* [Cassandra.] 
    ~DONE_ACCUSE = true
    -> FINISH_GAME_CASS
* [Jonathan.] 
    ~DONE_ACCUSE = true
    -> FINISH_GAME_JONATHAN
* [You.] 
    ~DONE_ACCUSE = true
    -> FINISH_GAME_DIRECTOR

->END

=== FINISH_GAME_CASS ===
I blamed Cassandra for the missing plinth.
She got fired, and eventually admitted that she stowed it away in the time capsule.
Sure enough, when we retrieved the time capsule, the plinth was there.
But with Cass gone, I became responsible for the exhibit.
I worked with both artifacts closely as the days blur by. Sometimes, in the corner of my eye, I see an ethereal person flitting by.
I want to keep the statue for myself... but I know what happens if I get caught.
Perhaps I can do better than Cass.
->END


=== FINISH_GAME_JONATHAN ===
I blamed Jonathan for the missing plinth.
Despite the Director's hesitance, he fired him.
I never saw Jonathan again. Last I heard, he was taking odd jobs to make ends meet.
We never found the plinth, either.
I started to regret blaming Jonathan... but he seemed the most suspicious from all my investigations.
Either way, the exhibit went pretty well--or as well as it would have while missing one artifact.
->END


=== FINISH_GAME_DIRECTOR ===
I blamed the director for the missing plinth.
After a long and arduous investigation, Director Virgil was proven innocent of any wrongdoing.
It didn't help that the plinth was nowhere to be found, and no clues other than Cassandra's keys in the director's office pointed to Director Virgil being the culprit.
While the investigation was ongoing, the director underwent some soul-searching, and realized that he was too obsessed with the founder, when she simply treated him as a colleague.
He entered marriage counseling with his wife, and eventually resolved their conflicts, albeit slowly.
After the investigation, I got fired for making baseless accusations.
When I left the museum, I took one last look at the building.
Something about the garden beds didn't feel right...
Oh well. It's not my business anymore.
->END