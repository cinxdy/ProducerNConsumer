# Producer and Consumer Program

# A bank system, which is implemented as a model of producer and consumer.

Like a bank in usual, several clients come into the bank, wait for the line, and then employees take their order,  
and then finally clients leave the bank after their work have been processed.

Such a model, where tasks are processed asynchronously, is known as producer and consumer problem, or bounded queue problem.


# Test
- Thread ratio 1:1 , waitingLineCapacity = 5
- Thread ratio 1:3 , waitingLineCapacity = 5
- Thread ratio 3:1 , waitingLineCapacity = 5
- Thread ratio 3:3 , waitingLineCapacity = 5
- Gracefully exiting.

# License
Jiyoung Shin