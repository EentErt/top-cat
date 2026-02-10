extends CharacterBody2D

var platforms = []

enum MoodType { Playful, Curious, Bored, Lazy }
enum EnergyLevel { Hyper, Active, Sleepy }

var target = null

var energy := 75
var mood := 50
var speed := 0
var mood_timer := 0.0
var sleeping := false
var squish := false

var direction := 1
var bounds := DisplayServer.window_get_size()
var width := 99
var height := 93

signal behavior(energy_mood)

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	platforms = _get_platforms()
	self.position.x = bounds[0] / 2.0
	self.position.y = bounds[1] - height

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	mood_timer += delta
	if mood_timer > 20.0:
		change_mood(int(randf() * 50.0))
		print("feeling %s and %s" % [describe_mood(), describe_energy_level()])
		if sleeping:
			energy = 100
			sleeping = false
		mood_timer = 0.0
		energy -= 5
	act()

# act based on mood
func act():
	# get action
	if squish:
		speed = 0
		$AnimatedCat.squish()
		return
	var action = get_behavior()
	behavior.emit([get_energy_level(), get_mood()])
	action.call()
	
# get behavior from behavior matrix
func get_behavior() -> Callable:
	var behaviors = [
	["play", "play", "wander", "sit"],
	["play", "wander", "sit", "sit"],
	["loaf", "loaf", "nap", "nap"]
	]
	return Callable(self, behaviors[get_energy_level()][get_mood()])
	
	
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
			
func play():
	speed = 100
	var mouse = get_global_mouse_position()
	$AnimatedCat.walk()
	$AnimatedCat/Head.look(mouse)
	if mouse.x - 100 > self.position.x or mouse.x + 100 < self.position.x:
		if int(mouse.x) > self.position.x:
			direction = 1
		elif int(mouse.x) < self.position.x:
			direction = -1
	else:
		sit()
	if direction == 1:
		$AnimatedCat.flip_h = true
	else:
		$AnimatedCat.flip_h = false
	
func wander():
	speed = 50
	if !target:
		target = Vector2(bounds[0] * randf(), bounds[1] * randf())
	$AnimatedCat.walk()
	$AnimatedCat/Head.look(target)
	if target.x > self.position.x:
		direction = 1
	else:
		direction = -1
	if round(target.x) == round(self.position.x):
		sit()
	if direction == 1:
		$AnimatedCat.flip_h = true
	else:
		$AnimatedCat.flip_h = false
	
func sit():
	speed = 0
	$AnimatedCat.sit()
	$AnimatedCat/Head.look(get_global_mouse_position())
	
	
func loaf():
	speed = 0
	$AnimatedCat.loaf()
	$AnimatedCat/Head.look(get_global_mouse_position())
	
func nap():
	speed = 0
	sleeping = true
	$AnimatedCat.nap()
	
func _get_platforms():
	platforms = $WindowDetector.GetWindowPlatforms()
	
func _on_timer_timeout() -> void:
	platforms = _get_platforms()
	
