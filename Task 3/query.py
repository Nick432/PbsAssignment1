import argparse
import pika
import json

class Query:
    def __init__(self, middleware_endpoint, person_id):
        self.person_id = person_id
        self.connection = pika.BlockingConnection(pika.ConnectionParameters(middleware_endpoint))
        self.channel = self.connection.channel()
        self.setup_queues()

    def setup_queues(self):
        self.channel.exchange_declare(exchange='topic_exchange', exchange_type='topic')
        self.channel.queue_declare(queue='query_response_queue')

    def send_query(self):
        self.channel.basic_publish(exchange='topic_exchange', routing_key='query', body=self.person_id)
        print(f"Sent query for {self.person_id}")

    def process_response(self, ch, method, properties, body):
        response = json.loads(body)
        person = response['person']
        contacts = response['contacts']
        print(f"Contacts for {person}: {', '.join(contacts)}")

    def start_listening(self):
        print("Waiting for query response...")
        self.channel.basic_consume(queue='query_response_queue', on_message_callback=self.process_response, auto_ack=True)
        self.channel.start_consuming()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Query for contacts of a person in the environment')
    parser.add_argument('person_name', type=str, help='Name of the person to query for')
    args = parser.parse_args()

    middleware_endpoint = "localhost"  # Replace with your RabbitMQ server's IP or hostname
    query = Query(middleware_endpoint, args.person_name)
    try:
        query.send_query()
        query.start_listening()
    except KeyboardInterrupt:
        print("Query stopped by user.")
    finally:
        query.connection.close()
