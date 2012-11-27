import com.rabbitmq.client.ConnectionFactory;
import com.rabbitmq.client.Connection;
import com.rabbitmq.client.Channel;
import com.rabbitmq.client.QueueingConsumer;

interface IWFMessageQueue
{
	void Dequeue(Object t);
	void Enqueue(Object t);
	int getCount();
	String getPath();
}

interface IWFMessageQueueT<T> extends IWFMessageQueue
{
	void Dequeue(T t);
	void Enqueue(T t);
}
	
public class Recv 
{

	private final static String QUEUE_NAME = "logs";

  public static void main(String[] argv) throws Exception 
  {
	  ConnectionFactory factory = new ConnectionFactory();
		factory.setHost("localhost");
		Connection connection = factory.newConnection();
	  Channel channel = connection.createChannel();
	  channel.exchangeDeclare("direct-exchange-example", "direct");
	
		channel.queueDeclare(QUEUE_NAME, false, false, false, null);
		channel.queueBind(QUEUE_NAME, "direct-exchange-example", "");
		System.out.println(" [*] Waiting for messages. To exit press CTRL+C");
    
	  QueueingConsumer consumer = new QueueingConsumer(channel);
		channel.basicConsume(QUEUE_NAME, true, consumer);
    
	  while (true) 
		{
			QueueingConsumer.Delivery delivery = consumer.nextDelivery();
			String message = new String(delivery.getBody());
	    System.out.println(" [x] Received '" + message + "'");
		}
  }
}