extends AnimatedSprite2D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	frame = 0

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
	
func look(location: Vector2) -> void:
	# angle to look
	if global_position.distance_to(location) < 100:
		frame = 5
		return
	var direction = int(rad_to_deg((location - global_position).angle()) + 180) % 360
	if direction >= 160 and direction <= 200:
		flip_h = true
		frame = 4
	elif direction < 160 and direction > 120:
		flip_h = true
		frame = 3
	elif direction <= 120 and direction > 90:
		flip_h = true
		frame = 2
	elif direction <= 90 and direction >= 60:
		flip_h = false
		frame = 2
	elif direction < 60 and direction > 20:
		flip_h = false
		frame = 3
	elif direction >= 340 or direction <= 20:
		flip_h = false
		frame = 4
	else:
		frame = 5
	
