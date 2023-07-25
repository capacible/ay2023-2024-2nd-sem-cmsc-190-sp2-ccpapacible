VAR divorce=false
VAR dog=false

Last week, I went on a vacation.
I needed it, especially after...
+ [ The death of Finna, my dog of 15 years. ]
    -> CHOICE_DEAD_DOG
+ [ My wife left me. ]
    -> CHOICE_DIVORCE

=== CHOICE_DEAD_DOG
~dog=true
... a few months back. #effect_add_to_player:PlayerDeadDog
    -> AFTER_CHOICE_1
-> END

=== CHOICE_DIVORCE
~divorce=true
... a few months back. #effect_add_to_player:PlayerDivorced
    ->AFTER_CHOICE_1
->END

=== AFTER_CHOICE_1
It felt like I wasn't myself at the time, but I couldn't fix it, because I was busy putting together my exhibit.
During those hellish months, I started to lean on...
+ [ Jonathan, the security guard. ]
    ->CHOICE_JONATHAN_CLOSE
+ [ Cassandra, my fellow curator. ]
    ->CHOICE_CASS_CLOSE
+ [ Annie, the director's assistant. ]
    -> CHOICE_ASSISTANT_CLOSE
->END

=== CHOICE_JONATHAN_CLOSE
We were just friends, of course. He would ask me about the exhibit I was putting together, about {divorce: how I was holding up after the divorce}{dog: what kind of dog Finna used to be}. #effect_set_rel:main_jonathan!25
He was a comforting presence in a time of my deep and profound sadness.
Despite the support, I found working to be unbearable.
Waking up... rather, getting out of bed... it was difficult.
{divorce: My wife used to be my entire world. With her gone, my world fell into a standstill.}{dog: My dog has been with me since I was a child. The memories of the time we spent haunted me day and night, and I couldn't be with him ever again}
Eventually, I decided...
+ [ To isolate myself from everyone. ]
    -> ISOLATION
+ [ To surround myself with more people, hoping to numb myself.  ]
    -> NUMBNESS

    = ISOLATION
    It didn't work. #effect_modify_rel:main_jonathan!-15 #effect_modify_rel:main_cassandra!-5 #effect_modify_rel:assistant!-5 #effect_add_to_global:UsedToBeFriendsWithJonathan
    Now, I had less people to help me. I had less avenues to feel better.
    I thought that being alone will help me process my pain, and that it would preserve what people think of me--as a cheerful curator, not the mess that I currently am.
    -> AFTER_CHOICE_3
    ->DONE
    
    = NUMBNESS
    I still felt so much pain. #effect_modify_rel:main_cassandra!5 #effect_modify_rel:assistant!5
    In frustration, I would lash out more. Every night, I'd ask the same question: why do I still feel like this? I did everything.
    ->AFTER_CHOICE_3
    ->DONE
->END

=== CHOICE_CASS_CLOSE
Even though we had a large age gap, working with Cassandra felt pleasant and comforting. #effect_set_rel:main_cassandra!25
Her wisdom allowed me to be more comfortable with my loss, as she's experienced a fair amount of it as well.
Despite the support, I found working to be unbearable.
Waking up... rather, getting out of bed... it was difficult.
{divorce: My wife used to be my entire world. With her gone, my world fell into a standstill.}{dog: My dog has been with me since I was a child. The memories of the time we spent haunted me day and night, and I couldn't be with him ever again}
Eventually, I decided...
+ [ To isolate myself from everyone. ]
    -> ISOLATION
+ [ To surround myself with more people, hoping to numb myself.  ]
    -> NUMBNESS

    = ISOLATION
    It didn't work. #effect_modify_rel:main_jonathan!-5 #effect_modify_rel:main_cassandra!-15 #effect_modify_rel:assistant!-5 #effect_add_to_global:UsedToBeFriendsWithCassandra
    Now, I had less people to help me. I had less avenues to feel better.
    I thought that being alone will help me process my pain, and that it would preserve what people think of me--as a cheerful curator, not the mess that I currently am.
    -> AFTER_CHOICE_3
    ->DONE
    
    = NUMBNESS
    I still felt so much pain. #effect_modify_rel:main_jonathan!5 #effect_modify_rel:assistant!5
    In frustration, I would lash out more. Every night, I'd ask the same question: why do I still feel like this? I did everything.
    ->AFTER_CHOICE_3
    ->DONE
->END

=== CHOICE_ASSISTANT_CLOSE
Annie was awkward, but she slowly started warming up the more we worked together.#effect_set_rel:assistant!25
We'd often go out for coffee and talk about things that helped me forget about my own problems. Despite being around half a decade younger than me, she was whip-smart.
She knew how to make me feel better when I felt especially down.
Despite the support, I found working to be unbearable.
Waking up... rather, getting out of bed... it was difficult.
{divorce: My wife used to be my entire world. With her gone, my world fell into a standstill.}{dog: My dog has been with me since I was a child. The memories of the time we spent haunted me day and night, and I couldn't be with him ever again}
Eventually, I decided...
+ [ To isolate myself from everyone. ]
    -> ISOLATION
+ [ To surround myself with more people, hoping to numb myself.  ]
    -> NUMBNESS

    = ISOLATION
    It didn't work. #effect_modify_rel:main_jonathan!-5 #effect_modify_rel:main_cassandra!-5 #effect_modify_rel:assistant!-15 #effect_add_to_global:UsedToBeFriendsWithAssistant
    Now, I had less people to help me. I had less avenues to feel better.
    I thought that being alone will help me process my pain, and that it would preserve what people think of me--as a cheerful curator, not the mess that I currently am.
    -> AFTER_CHOICE_3
    ->DONE
    
    = NUMBNESS
    I still felt so much pain. #effect_modify_rel:main_cassandra!5 #effect_modify_rel:main_jonathan!5
    In frustration, I would lash out more. Every night, I'd ask the same question: why do I still feel like this? I did everything.
    ->AFTER_CHOICE_3
    ->DONE
->END

=== AFTER_CHOICE_3
I can feel myself changing, and not in the way that I wanted.
And so I went on a vacation.
If not to find myself, but to remove myself from people I can potentially lash out on.
I don't want the people I know to see me like the grief-struck person I was.
...
My vacation is over now.
It's time to face the music.
I stepped into the museum with a bit of dread. Frankly, I'm worried that despite my absence, people would still change their opinion of me. That they'd tiptoe around me like before, acting like I'm the most fragile artifact of the museum.
Thankfully they didn't. Not much, anyway.
But something else happened while I was gone.
I started preparing for my workday by checking on the artifacts in the storage unit.
A plinth for the next exhibit... it's missing. #effect_add_to_player:ArtifactNotFound #active_ink:false
->END