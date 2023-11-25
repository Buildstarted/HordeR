
class Camera {
    constructor(x, y, width, height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.entities = [];
        this.target = { x: 0, y: 0 };
    }

    follow(entity) {
        this.target = entity;
    }

    update() {
        for (let i in this.entities) {
            this.entities[i].update();
        }
    }

    draw() {
        $context.clearRect(0, 0, this.width, this.height);
        if (this.target) {

            let xOffset = -this.target.x;
            let yOffset = -this.target.y;

            this.x = xOffset + this.width / 2;
            this.y = yOffset + this.height / 2;

            $context.setTransform(1, 0, 0, 1, this.x, this.y);
            $context.save();
        }
        for (let i in this.entities) {
            this.entities[i].draw();
        }
        if (this.target) {
            $context.restore();
        }
        //ensure we set the transform back to what it should be
        $context.setTransform(1, 0, 0, 1, 0, 0);
    }

    addEntity(entity) {
        this.entities.push(entity);
    }

    removeEntity(entity) {
        const index = this.entities.indexOf(entity);
        if (index > -1) {
            this.entities.splice(index, 1);
        }
    }
}

class Player {
    constructor(x, y, radius, color, name) {
        this.x = x;
        this.y = y;
        this.radius = radius;
        this.color = color;
        this.name = name;
        this.lastJump = 0;
        this.jumpReleased = true;
        this.speed = 5;
        this.revived = true;
    }

    jumpAvailable() {
        return this.jumpReleased && this.lastJump + 5000 < performance.now();
    }

    jump() {
        this.jumpReleased = false;
        this.jumpAvailable = false;
        this.lastJump = performance.now(); //need to wait 5 seconds
    }

    update() {
        if (this.target) {
            let nextX = this.x + (this.target.x - this.x) * .5;
            let nextY = this.y + (this.target.y - this.y) * .5;

            if (this.revived) {
                nextX = this.target.x;
                nextY = this.target.y;
            }

            for (const id in obstacles) {
                const obstacle = obstacles[id];
                if (nextX + this.radius > obstacle.x && nextX - this.radius < obstacle.x + obstacle.width && this.y + this.radius > obstacle.y && this.y - this.radius < obstacle.y + obstacle.height) {
                    nextX = this.x;
                }

                if (this.x + this.radius > obstacle.x && this.x - this.radius < obstacle.x + obstacle.width && nextY + this.radius > obstacle.y && nextY - this.radius < obstacle.y + obstacle.height) {
                    nextY = this.y;
                }
            }

            this.x = nextX;
            this.y = nextY;
        }
    }

    draw() {
        $context.font = '12px sans-serif';
        $context.fillStyle = 'white';
        $context.textAlign = 'center';
        $context.fillText(this.name, this.x, this.y - 25);
        $context.save();
        $context.shadowColor = this.color;
        $context.shadowBlur = 20;
        $context.beginPath();
        $context.arc(this.x, this.y, this.radius, 0, Math.PI * 2, false);
        $context.fillStyle = this.color;
        $context.fill();
        $context.restore();
    }
}
