import pika
import json
from collections import defaultdict

class Tracker:
    def __init__(self, middleware_endpoint):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters(middleware_endpoint))
        self.channel = self.connection.channel()
        self.setup_queues()
        self.environment = defaultdict(list)

    def setup_queues(self):
        self.channel.exchange_declare(exchange='topic_exchange', exchange_type='topic')
        self.channel.queue_declare(queue='position_queue')
        self.channel.queue_declare(queue='query_queue')
        self.channel.queue_declare(queue='query_response_queue')
        self.channel.queue_bind(exchange='topic_exchange', queue='position_queue', routing_key='position')
        self.channel.queue_bind(exchange='topic_exchange', queue='query_queue', routing_key='query')

    def handle_position_message(self, ch, method, properties, body):
        position_data = json.loads(body)
        person_id = position_data['person_id']
        position = tuple(position_data['position'])
        self.environment[position].append(person_id)
        self.check_contact(position, person_id)

    def check_contact(self, position, person_id):
        if len(self.environment[position]) > 1 and person_id not in self.environment[position]:
            contacts = self.environment[position].copy()
            contacts.remove(person_id)
            print(f"Contacts at position {position}: {', '.join(contacts)}")


    def publish_contact_response(self, person, contacts):
        response = {'person': person, 'contacts': list(contacts)}
        self.channel.basic_publish(exchange='topic_exchange', routing_key='query_response', body=json.dumps(response))

    def handle_query_message(self, ch, method, properties, body):
        person_id = body.decode()
        if person_id in self.environment:
            contacts = self.environment[person_id]
        else:
            contacts = []
        self.publish_contact_response(person_id, contacts)

    def start_consuming(self):
        print("Tracker is running...")
        self.channel.basic_consume(queue='position_queue', on_message_callback=self.handle_position_message, auto_ack=True)
        self.channel.basic_consume(queue='query_queue', on_message_callback=self.handle_query_message, auto_ack=True)
        self.channel.start_consuming()

if __name__ == "__main__":
    middleware_endpoint = "localhost"  # Replace with your RabbitMQ server's IP or hostname
    tracker = Tracker(middleware_endpoint)
    try:
        tracker.start_consuming()
    except KeyboardInterrupt:
        print("Tracker stopped by user.")
    finally:
        tracker.connection.close()
