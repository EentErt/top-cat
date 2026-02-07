extends Node2D

var usable

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	usable = DisplayServer.screen_get_usable_rect()
	$Cat.position.x = (usable.position.x + usable.size.x) / 2
	$Cat.position.y = (usable.position.y + usable.size.y) - 100


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	$Cat.position.x += $Cat.speed * $Cat.direction * delta
