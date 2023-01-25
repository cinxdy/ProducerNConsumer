# Producer and Consumer Program

## A bank system, which is implemented as a model of producer and consumer.

In our daily life in a bank, several clients come into the bank, wait for the line,  
and then one of several employees takes the order, then finally clients who finished their business leave the bank.

In this case, clients keep entering, and employees keep working asynchonously. Clients cannot come into a bank but have to wait for the line  
unless the waiting line is empty. Similarly, employees cannot provide any services but have to wait for clients unless there is any client.  
Such a model, where tasks are processed asynchronously, is known as producer and consumer problem, or bounded queue problem.

In this project, I implemented the producer and consumer model, simulating a bank system.


![IMG](./img.png)

# Test
- Thread ratio 1:1 , waitingLineCapacity = 5
- Thread ratio 1:3 , waitingLineCapacity = 5
- Thread ratio 3:1 , waitingLineCapacity = 5
- Thread ratio 3:3 , waitingLineCapacity = 5
- Gracefully exiting.

# License
Jiyoung Shin