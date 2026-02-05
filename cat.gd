extends CharacterBody2D

enum MoodType { Playful, Curious, Bored, Lazy }
enum EnergyLevel { Hyper, Active, Sleepy }

var energy := 75
var mood := 50
var speed := 0
var mood_timer := 0.0
var sleeping := false

var direction := 1
var bounds := DisplayServer.window_get_size()
var width := 100
var height := 100

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	self.position.x = bounds[0] / 2
	self.position.y = bounds[1] - height

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	mood_timer += delta
	if mood_timer > 20.0:
		change_mood(int(randf() * 50.0))
		print("feeling %s and %s" % [describe_mood(), describe_energy_level()])
		if sleeping:
			energy = 100
		mood_timer = 0.0
		energy -= 5
	if (int(self.position.x + 100) >= bounds[0]):
		direction = -1
	elif (self.position.x <= 0):
		direction = 1
	act()

# act based on mood
func act():
	match get_energy_level():
		EnergyLevel.Hyper:
			speed = 100
			$AnimatedCat.frame = 0
		EnergyLevel.Active:
			speed = 60
			$AnimatedCat.frame = 0
		_:
			$AnimatedCat.frame = 1
			speed = 0
			sleeping = true
	match get_mood():
		MoodType.Playful:
			pass
		MoodType.Curious:
			pass
		MoodType.Bored:
			pass
		MoodType.Lazy:
			$AnimatedCat.frame = 2
			speed = 0
		_:
			# turn into a cloud
			pass
	
# change mood by value
func change_mood(value) -> void:
	mood += value
	mood = mood % 100
	
# convert mood to MoodType	
func get_mood() -> MoodType:
	if (mood > 80):
		return MoodType.Playful
	elif (mood > 50):
		return MoodType.Curious
	elif (mood > 30):
		return MoodType.Lazy
	else:
		return MoodType.Bored
		
# describe the current mood
func describe_mood() -> String:
	match get_mood():
		MoodType.Playful:
			return "playful"
		MoodType.Curious:
			return "curious"
		MoodType.Bored:
			return "bored"
		MoodType.Lazy:
			return "lazy"
		_:
			return "mysterious"
	
func get_energy_level():
	if energy > 80:
		return EnergyLevel.Hyper
	elif energy > 50:
		return EnergyLevel.Active
	else:
		return EnergyLevel.Sleepy
		
func describe_energy_level():
	match get_energy_level():
		EnergyLevel.Hyper:
			return "hyper"
		EnergyLevel.Active:
			return "active"
		EnergyLevel.Sleepy:
			return "sleepy"
		_:
			return "incorporeal"
