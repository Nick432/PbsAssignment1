import argparse
import pika
import json
import random
import time

class Person:
    def __init__(self, middleware_endpoint, person_id, movement_speed):
        self.person_id = person_id
        self.movement_speed = movement_speed
        self.connection = pika.BlockingConnection(pika.ConnectionParameters(middleware_endpoint))
        self.channel = self.connection.channel()
        self.setup_queues()

    def setup_queues(self):
        self.channel.exchange_declare(exchange='topic_exchange', exchange_type='topic')

    def move(self):
        directions = [(0, 1), (0, -1), (1, 0), (-1, 0)]  # Up, Down, Right, Left
        while True:
            direction = random.choice(directions)
            new_position = self.calculate_new_position(direction)
            self.publish_position(new_position)
            time.sleep(1)  # Publish position every second

    def calculate_new_position(self, direction):
        # Current position assumed to be at (0, 0)
        new_position = (direction[0], direction[1])
        return new_position

    def publish_position(self, position):
        position_data = {'person_id': self.person_id, 'position': position}
        self.channel.basic_publish(exchange='topic_exchange', routing_key='position', body=json.dumps(position_data))
        print(f"Published position: {position_data}")

    def start_moving(self):
        print(f"Person {self.person_id} started moving...")
        self.move()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Simulate a person moving around the environment')
    parser.add_argument('person_name', type=str, help='Name of the person')
    args = parser.parse_args()

    middleware_endpoint = "localhost"  # Replace with your RabbitMQ server's IP or hostname
    movement_speed = 1  # Movement speed is 1 position per second

    person = Person(middleware_endpoint, args.person_name, movement_speed)
    try:
        person.start_moving()
    except KeyboardInterrupt:
        print(f"Person {args.person_name} stopped moving.")
    finally:
        person.connection.close()
